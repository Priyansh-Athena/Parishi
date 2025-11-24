using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GravityEngine2
{
    /// <summary>
	/// Specifies and controls a multi-stage rocket for launch from a planet
    /// surface. 
    /// 
    /// Specification:
    /// - the payload mass, number of stages and per-stage physical attributes
    /// (dry mass, fuel mass, thrust, burn time) for up to 4 stages.
    /// - display objects for each stage and payload. (Can be multiple for 
    /// each since can have multiple views e.g. world 3D and launch in profile)
    /// - steering law for ascent. This can be an parameterized automatic method
    /// or can have user-controlled pitch
    /// 
    /// Control:
    /// - optional key press to trigger launch (or Launch() API)
    /// - callback handler for staging and collision events
    /// - simple status reporting to indicate fuel level, velocity and position
    /// 
    /// Overview:
	/// 
	/// </summary>
    public class GSBoosterMultiStage : MonoBehaviour
    {

        [Header("L to Launch")]
        public GSBody payloadBody;

        public TMPro.TMP_Text statusText;

        public GSController gsController;

        public double displayOrbitAtHeightKm;


        public double payloadMassKg;

        public double payloadDragCoeff;
        public double payloadCrossSectionalArea;

        public bool stagePayload = true;

        public const int MAX_STAGES = 4;

        public int numStages;

        // Max 4 stages. Just alloc all
        public double[] dryMassKg = new double[MAX_STAGES];
        public double[] fuelMassKg = new double[MAX_STAGES];
        public double[] burnTimeSec = new double[MAX_STAGES];
        public double[] thrustN = new double[MAX_STAGES];

        public double[] dragCoeff = new double[MAX_STAGES];
        public double[] crossSectionalArea = new double[MAX_STAGES];

        public enum SteeringMode { MANUAL, LINEAR_TAN_EC, PITCH_TABLE, /* PEG_2D */ };
        public SteeringMode steeringMode = SteeringMode.MANUAL;
        public double steeringParam;
        public string pitchFile;

        public double targetInclDeg = 0.0;

        private double3 orbitNormal;

        public double targetAltitudeSI = 200000.0; // 200 km

        public bool stage1GravityTurn = true;

        public double startTurnAtVelocitySI = 10.0; // m/s

        public double startTurnPitchKickDeg = 2.0; // deg

        public double pitchRateDegPerSec = 1.0; // deg/s

        public bool earthAtmosphere;

        [Header("MANUAL use A/S to adjust pitch")]
        public bool manualKeys = true;

        private double3[] pitchTable;

        private GSBody centerBody;
        private int boosterExtAccelId;
        private int earthAtmoExtAccelId;

        private List<int> boosterBodyIds = new List<int>();

        private double moonMu;

        private bool launched = false;

        private bool displayOrbit = false;

        private List<GSStageDisplayMgr> stageDisplayManagers = new List<GSStageDisplayMgr>();

        public delegate void LaunchAsJobCallback(GEBodyState[] worldStates);

        private GECore gePreview;
        private int previewBoosterId;

        public delegate void LaunchPreviewCallback(GEBodyState[] worldStates, double3 orbitNormal);

        private List<LaunchPreviewCallback> launchPreviewCallbacks = new List<LaunchPreviewCallback>();

        public void RegisterStageDisplayManager(GSStageDisplayMgr manager)
        {
            if (!stageDisplayManagers.Contains(manager))
            {
                stageDisplayManagers.Add(manager);
            }
        }

        void Awake()
        {
            launchPreviewCallbacks = new List<LaunchPreviewCallback>();

            if (steeringMode == SteeringMode.PITCH_TABLE)
            {
                // assume the pitch table is a text file in the resources of the project. Name w/o .txt
                pitchTable = EphemerisLoader.LoadPitchTable(pitchFile);
            }
            centerBody = payloadBody.centerBody;
            if (centerBody == null)
            {
                Debug.LogError("Payload body must have a center body");
            }
        }

        // for now start with normal = (1,0,0) and rotate in +y direction
        // keep theta in range 0 (vertical) to Pi/2 (horizontal along y)
        private double pitchDeg = 90;
        private double dThetaDeg = 2.0;

        private int frameCnt = 0;

        // Update is called once per frame
        void Update()
        {
            if (gePreview != null)
            {
                if (gePreview.IsCompleted())
                {
                    gePreview.Complete();
                    ProcessPreview();
                    gePreview.Dispose();
                    gePreview = null;
                }
            }
            if (!launched)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // add the GSBody and the engine to GE
                    gsController.GECore().PhyLoopCompleteCallbackAdd(LaunchInScene);
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    LaunchPreview(centerBody);
                }
            }
            else
            {
                // launched
                if (Input.GetKeyDown(KeyCode.A))
                {
                    pitchDeg -= dThetaDeg;
                    gsController.GECore().PhyLoopCompleteCallbackAdd(DoPitch);
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    pitchDeg += dThetaDeg;
                    gsController.GECore().PhyLoopCompleteCallbackAdd(DoPitch);
                }
                // don't update every frame 
                if (frameCnt % 10 == 0)
                {
                    gsController.GECore().PhyLoopCompleteCallbackAdd(UIUpdate);
                }
                frameCnt++;
            }

        }

        public void RegisterLaunchPreviewCallback(LaunchPreviewCallback callback)
        {
            launchPreviewCallbacks.Add(callback);
        }

        private void ProcessPreview()
        {
            GEBodyState[] worldStates = gePreview.RecordedOutputForBody(previewBoosterId);
            Debug.Log("Last point: " + worldStates[worldStates.Length - 1].LogString());
            foreach (LaunchPreviewCallback callback in launchPreviewCallbacks)
            {
                callback(worldStates, orbitNormal);
            }
        }

        private void DoPitch(GECore ge, object notUsed = null)
        {
            double3[] data = ge.ExternalAccelerationData(boosterExtAccelId);
            Booster.ManualPitchUpdate(math.radians(pitchDeg), data);
            ge.ExternalAccelerationDataUpdate(boosterExtAccelId, data);
            Debug.LogFormat("set pitch={0}", pitchDeg);
        }


        private void UIUpdate(GECore ge, object notUsed = null)
        {
            if (statusText == null)
                return;

            // UI Update. To keep things simple just do a text string
            // get pos/vel in RSW frame
            // r_r, r_s, v_r, v_s, apo, peri
            GEBodyState lmState = new GEBodyState();
            GEBodyState moonState = new GEBodyState();
            ge.StateById(payloadBody.Id(), ref lmState);
            ge.StateById(centerBody.Id(), ref moonState);
            GEBodyState rswState = new GEBodyState();
            Orbital.RSWState(ref lmState, ref moonState, orbitNormal, ref rswState);
            double h = math.length(rswState.r) - GBUnits.earthRadiusKm;

            double3[] data = ge.ExternalAccelerationData(boosterExtAccelId);
            double pitchNow = pitchDeg;
            if (steeringMode != SteeringMode.MANUAL)
            {
                pitchNow = math.degrees(Booster.PitchReadback(data));
            }

            statusText.text = string.Format("v_r={0:##.###} km/sec\n v_s={1:##.###} km/sec\n h={2:G5}\n pitch={3}\n t={4}",
                      rswState.v.x, rswState.v.y, h, pitchNow, ge.TimeWorld());

            double fuel = Booster.FuelReadout(data);
            for (int i = 0; i < numStages; i++)
            {
                statusText.text += string.Format("\nFuel stage{0} = {1}", i, Booster.FuelReadout(data, i));
            }

            // GET COE if there is some angular momtm
            double3 r = lmState.r - moonState.r;
            double3 v = lmState.v - moonState.v;
            if (math.abs(math.length(math.cross(r, v))) > 1E-3)
            {
                if (!displayOrbit && math.length(r) > displayOrbitAtHeightKm + centerBody.radius)
                {
                    displayOrbit = true;
                    foreach (GSStageDisplayMgr manager in stageDisplayManagers)
                    {
                        manager.DisplayOrbitSet(true);
                    }
                }
                Orbital.COE coe =
                    Orbital.RVtoCOE(r, v, moonMu);
                (double apo, double peri) = coe.ApoPeri();
                if (!double.IsNaN(apo) && !double.IsNaN(peri))
                    statusText.text += string.Format("\nApolune={0}\nPerilune={1}", apo, peri);
            }
        }


        /// <summary>
        /// Launch preview. Evolve the rocket engine until the fuel runs out. Record the positions
        /// and return the state of the booster.
        /// 
        /// Creates a new GECore instance for the preview and runs it directly. Can be a bit slow.
        /// 
        /// If a callback is provided the sim will run in job mode and then call the callback with the
        /// states. This is more efficient if you need to do a lot of previews.
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="callback">Callback to call when the preview is complete.</param>
        /// <returns>The states of the booster at the end of the preview (if immediate).</returns>
        public GEBodyState[] LaunchPreview(GSBody planet)
        {
            int id = payloadBody.Id();
            GEBodyState bodyState = new GEBodyState();
            gsController.GECore().StateById(id, ref bodyState);

            // get the params for the GECore and create one for the preview
            gePreview = new GECore(gsController.integrator,
                                    gsController.defaultUnits,
                                    gsController.orbitScale,
                                    gsController.orbitMass,
                                    gsController.stepsPerOrbit);
            // Add just the planet and the launch vehicle
            gePreview.BodyAdd(planet.bodyInitData.r, planet.bodyInitData.v, massWorld: planet.mass, isFixed: true);

            if (payloadBody.bodyInitData.initData == BodyInitData.InitDataType.LATLONG_POS)
            {
                double latitudeDeg = payloadBody.bodyInitData.latitude;
                if (latitudeDeg > targetInclDeg)
                {
                    Debug.LogWarning("Target incl. must be >= latitude. Limiting to latitude");
                    targetInclDeg = latitudeDeg;
                }
                orbitNormal = Orbital.LaunchPlane(targetInclDeg, bodyState.r, latitudeDeg);
                Debug.LogFormat("Launch plane={0}", orbitNormal);
            }
            else
            {
                Debug.LogWarning("expected payload init mode LATLONG_POS");
            }
            // add booster
            previewBoosterId = gePreview.BodyAddInOrbitWithRVRelative(bodyState.r, bodyState.v, centerBody.Id(), prop: GEPhysicsCore.Propagator.GRAVITY);
            gePreview.ColliderAddToBody(id, radius: 0.001, bounceF: 0.0, GEPhysicsCore.CollisionType.TRIGGER, mass: 0.0);
            moonMu = gePreview.MuWorld(centerBody.Id());
            AddEngine(id, gePreview, gsController.defaultUnits);
            // Evolve the rocket engine until the fuel runs out. Record the positions
            double3[] data = gePreview.ExternalAccelerationData(boosterExtAccelId);
            double burnTime = Booster.TotalBurnTime(data);
            double[] times = new double[(int)burnTime];
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = i;
            }
            // FOR DEBUGGING
            // gePreview.ScheduleRecordOutput(burnTime, times, new int[] { previewBoosterId });
            gePreview.EvolveNowRecordOutput(burnTime, times, new int[] { previewBoosterId });
            return null;
        }

        private void LaunchInScene(GECore ge, object p = null)
        {
            if (launched)
                return;

            int id = payloadBody.Id();
            GEBodyState bodyState = new GEBodyState();
            ge.StateById(id, ref bodyState);
            if (payloadBody.bodyInitData.initData == BodyInitData.InitDataType.LATLONG_POS)
            {
                double latitudeDeg = payloadBody.bodyInitData.latitude;
                if (latitudeDeg > targetInclDeg)
                {
                    Debug.LogWarning("Target incl. must be >= latitude. Limiting to latitude");
                    targetInclDeg = latitudeDeg;
                }
                orbitNormal = Orbital.LaunchPlane(targetInclDeg, bodyState.r, latitudeDeg);
                Debug.LogFormat("Launch plane={0}", orbitNormal);
            }
            else
            {
                Debug.LogWarning("expected payload init mode LATLONG_POS");
            }
            // Remove the LM (it was added ith a fixed LAT LONG position) and then re-add it.
            // It is guaranteed to get same id so display reference will be ok. 
            ge.BodyRemove(id);
            // add as a massless body governed by gravity
            int idNew = ge.BodyAddInOrbitWithRVRelative(bodyState.r, bodyState.v, centerBody.Id(), prop: GEPhysicsCore.Propagator.GRAVITY);
            ge.ColliderAddToBody(id, radius: 0.001, bounceF: 0.0, GEPhysicsCore.CollisionType.TRIGGER, mass: 0.0);
            boosterBodyIds.Add(idNew);
            // subtle point. The way GECore is coded a re-add after a delete will re-use the same
            // ID. To make this clear, here we check it is the case. Unit tests also cover this. 
            if (idNew != id)
            {
                Debug.LogError("FATAL error. Code relies on re-add id not changing!!");
            }
            payloadBody.propagator = GEPhysicsCore.Propagator.GRAVITY;
            moonMu = ge.MuWorld(centerBody.Id());
            AddEngine(id, ge, gsController.defaultUnits);
            ge.PhysicsEventListenerAdd(BoosterEventCallback);
            // Notify stage display managers about the launch
            foreach (GSStageDisplayMgr manager in stageDisplayManagers)
            {
                manager.OnLaunch();
            }
            Debug.Log("Launched at t=" + ge.TimeWorld() + " go=" + gameObject.name);
            launched = true;
        }

        private void AddEngine(int id, GECore ge, GBUnits.Units units)
        {
            double3 planetCenter = double3.zero;

            // convert thrust to default units, then to GE
            // launch and burn times
            double3[] data = null;
            switch (steeringMode)
            {
                case SteeringMode.MANUAL:
                    data = Booster.ManualPitchAlloc(pitchRad: math.radians(pitchDeg), numStages: numStages);
                    break;

                case SteeringMode.PITCH_TABLE:
                    data = Booster.PitchTableAlloc(pitchTable, numStages: numStages);
                    break;

                case SteeringMode.LINEAR_TAN_EC:
                    data = Booster.LinearTangentECAlloc(steeringParam, numStages);
                    break;

                    // case SteeringMode.PEG_2D:
                    //     data = Booster.Peg2DAlloc(numStages: numStages,
                    //                               mode: Booster.PEG2DMode.PEG_AT_START,
                    //                               gt_at_vel: startTurnAtVelocitySI,
                    //                               gt_pitch_kick: math.radians(startTurnPitchKickDeg),
                    //                               pitch_rate: math.radians(pitchRateDegPerSec),
                    //                               target_altitude: targetAltitudeSI,
                    //                               target_vz: 0.0,
                    //                               cycle_time: 1.0);
                    //     break;

            }
            Booster.EnabledSet(data, true);
            Booster.AutoStageSet(data, true);

            // Add the stages
            double totalBurnTime = 0.0;
            for (int i = 0; i < numStages; i++)
            {
                Booster.Stage stage = new Booster.Stage
                {
                    mass_dry = dryMassKg[i], // dry mass + mass of upper stage (dry_mass + fuel)
                                             // give first stage double fuel of second
                    mass_fuel = fuelMassKg[i],
                    thrustN = thrustN[i],
                    burn_time_sec = burnTimeSec[i],
                };

                Booster.StageSetup(data,
                                stageNum: i,
                                stage);
                totalBurnTime += burnTimeSec[i];
            }
            Booster.PayloadCompute(data, payloadMassKg);
            double t_start = ge.TimeWorld();
            double t_final = t_start + totalBurnTime;
            Booster.ThrottleSet(data, 1.0);
            Booster.SteeringPlaneSet(data, planetCenter, orbitNormal);
            Booster.SteeringTimesSet(data, t_start, t_final);

            boosterExtAccelId = ge.ExternalAccelerationAdd(id,
                                                ExternalAccel.ExtAccelType.SELF,
                                                ExternalAccel.AccelType.BOOSTER,
                                                data);

            uint boosterDataOffset = (uint)ge.ExternalAccelerationDataOffset(boosterExtAccelId);
            // Add an SI Earth atmosphere model that takes the mass from the booster
            if (earthAtmosphere)
            {
                uint massLU = (boosterDataOffset << 2) + 3; // 3 => .z field
                double3[] dataEA = EarthAtmosphere.Alloc(GBUnits.earthRadiusKm, dragCoeff[0], crossSectionalArea[0], 1.0, massLU);
                earthAtmoExtAccelId = ge.ExternalAccelerationAdd(id,
                                            ExternalAccel.ExtAccelType.SELF,
                                            ExternalAccel.AccelType.EARTH_ATMOSPHERE,
                                            dataEA);
            }
        }

        private List<int> stagesAdded = new List<int>();

        /// <summary>
        /// Handle a physics event from GECore. This may be one of:
        /// - BOOSTER event indicating staging
        /// - COLLISION event indicating a stage or payload has hit the ground
        /// 
        /// The code for managing staging is contained here. There are seperate models
        /// for each configuration of the booster (all stages, after first stage drops etc.). 
        /// A stage being dropped needs to be added as an independent object in the GECore and
        /// GSDisplay elements. This body also needs to have the atmosphere act on it. 
        /// </summary>
        /// <param name="ge"></param>
        /// <param name="pEvent"></param>
        private void BoosterEventCallback(GECore ge, GEPhysicsCore.PhysEvent pEvent)
        {
            if (pEvent.type == GEPhysicsCore.EventType.COLLISION)
            {
                // may get repeat collision report since it is added in Trigger mode
                int collisionId = -1;
                bool primary = true;
                if (boosterBodyIds.Contains(pEvent.bodyId))
                {
                    collisionId = pEvent.bodyId;
                }
                else if (boosterBodyIds.Contains(pEvent.bodyId_secondary))
                {
                    collisionId = pEvent.bodyId_secondary;
                    primary = false;
                }
                if (collisionId == -1)
                {
                    Debug.LogError("Collision event with unknown body");
                    return;
                }
                // A collision will trigger at the start before the lm is moving. In this case the velocity of the lm
                // will be upward, so if that's true, do nothing. For simplicity assume moon at r=0, v=0
                if (primary && math.dot(pEvent.r, pEvent.v) >= 0.0)
                {
                    return;
                }
                if (!primary && math.dot(pEvent.r_secondary, pEvent.v_secondary) >= 0.0)
                {
                    return;
                }
                ge.BodyRemove(collisionId);
                boosterBodyIds.Remove(collisionId);

            }
            else if (pEvent.type == GEPhysicsCore.EventType.BOOSTER)
            {
                double3[] data = ge.ExternalAccelerationData(boosterExtAccelId);
                int stageNum;
                // Handle staging on either a stage out or fuel out
                if (pEvent.statusCode == Booster.STATUS_STAGED)
                {
                    stageNum = Booster.ActiveStageReadback(data) - 1;
                    Debug.LogFormat("Stage {0} out at t={1} ", stageNum, ge.TimeWorld());
                }
                else if (pEvent.statusCode == Booster.FUEL_OUT)
                {
                    // fuel out means the final stage has run out of fuel
                    stageNum = numStages - 1;
                    GEBodyState stageState = new GEBodyState();
                    ge.StateById(payloadBody.Id(), ref stageState);
                    Debug.LogFormat("FUEL OUT Stage {0} out at t={1} r={2} v={3}", stageNum, ge.TimeWorld(), stageState.r, stageState.v);
                    // TODO remove ext accel
                }
                else
                {
                    Debug.Log("Unknown event type from Booster " + pEvent.type);
                    return;
                }
                if (stageNum < numStages && !stagesAdded.Contains(stageNum))
                {
                    if (stageNum == numStages - 1 && !stagePayload)
                    {
                        // payload stays attached to last stage, or is just a single body with an engine
                        return;
                    }
                    // Drop a stage
                    // Add a GSBody to handle the dropped stage object
                    // Staging:
                    // 1) Get the state of the booster
                    GEBodyState stageState = new GEBodyState();
                    ge.StateById(payloadBody.Id(), ref stageState);
                    // 2) Add to GECore, get new body id & add a collider and EarthAtmosphere model. Note we are doing this
                    // without creating a GSBody for the stage.
                    int stageId = ge.BodyAdd(stageState.r, stageState.v);
                    boosterBodyIds.Add(stageId);
                    ge.ColliderAddToBody(stageId, radius: 0.001, bounceF: 0.0, GEPhysicsCore.CollisionType.TRIGGER, mass: 0.0);
                    // EarthAtmosphere for the  dropped stage
                    uint massLU = 0;
                    double3[] dataEA = EarthAtmosphere.Alloc(GBUnits.earthRadiusKm, dragCoeff[stageNum], crossSectionalArea[stageNum], dryMassKg[stageNum], massLU);
                    ge.ExternalAccelerationAdd(stageId,
                                                ExternalAccel.ExtAccelType.SELF,
                                                ExternalAccel.AccelType.EARTH_ATMOSPHERE,
                                                dataEA);

                    // Adjust atmosphere model for the current stack

                    // Tell staging managers about the new stage
                    foreach (GSStageDisplayMgr manager in stageDisplayManagers)
                    {
                        manager.OnStageChange(stageNum, stageId);
                    }
                    Debug.LogFormat("Staging dropping stage={0} bodyId={1}", stageNum, stageId);

                    // if this is the final stage, remove the Booster and set EarthAtm to payload mass
                    if (stageNum == numStages - 1)
                    {
                        ge.ExternalAccelerationRemove(boosterExtAccelId);
                        if (earthAtmosphere)
                        {
                            double3[] dataPayload = EarthAtmosphere.Alloc(GBUnits.earthRadiusKm, payloadDragCoeff, payloadCrossSectionalArea, payloadMassKg, massLU);
                            ge.ExternalAccelerationDataUpdate(earthAtmoExtAccelId, dataPayload);
                        }
                    }
                }

            }
        }

    }
}
