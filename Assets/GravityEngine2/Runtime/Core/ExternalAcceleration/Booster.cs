using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace GravityEngine2 {
    /// <summary>
	/// Booster External Acceleration
	///
	/// This class provides the external acceleration of a multi-stage booster. It will be called for each timestep in the
	/// numerical integration. Booster also provides a set of methods to assist in determining the launch trajewctory and
    /// end state without being used as part of GEPhysicsCore. This allows tuning of booster parameters ahead of launch.
	///
	/// This implementation supports:
	/// - multiple stages that (may) reduce the mass of the booster as time goes on (i.e. rocket equation stuff)
	/// - a choice of steering laws for the direction of the thrust: manual vector, manual pitch in a target plane,
	///   tangent steering in target plance etc.
	///
	/// Booster setup allocates a block of double3[] to hold the state information required for the stages and steering.
    /// This ensures it can be run within the Job system and can be Burst compiled.
    /// 
	/// 
	/// </summary>
    [BurstCompile]
    public class Booster {

        public enum Guidance { LINEAR_TANGENT_EC, MANUAL_VEC, MANUAL_PITCH, PITCH_TABLE, PEG_2D, PEG2D_GT, UNUSED };

        // Status return codes
        // FUEL_OUT is set when the fuel for the final stage is depleted
        // STAGED is set when the a stage is separated
        public const int STATUS_FUEL_OUT = 1;
        public const int STATUS_STAGED = 2;

        // control fields in the control data entry
        public const int ENABLED = 1 << 0;
        public const int AUTO_STAGE = 1 << 1;
        public const int SEND_EVENTS = 1 << 2;

        // ext accel status field
        public const int FUEL_OUT = 1 << 0;
        public const int STAGED = 1 << 1;

        private const int HEADER_SIZE = 2;  // 2 entries for preamble
        private const int STAGE_SIZE = 2;
        private const int GUID_SIZE = 3;
        ///
        /// Generic Booster Data Block for a stage with (optional) mass reduction as fuel is consumed
        /// data block is:
        /// [0] = (control, status, currentMassKg)
        /// [1] = (numStages, activeStage, throttle)
        /// stage 0
        /// [2] = (mass_dry, mass_fuel, mass_payload)
        /// [3] = (thrust_full, fuel_rate, ISP)
        /// stage 1 (2 entries)
        /// ...
        /// Offset from STEER_BASE = HEADER_SIZE+N*STAGENUM
        /// [0] (steeringLaw, tStart, tEnd) 
        /// [+1] = (planet center)
        /// [+2] = (orbit plane normal)
        /// [+3...] (optional steering data e.g. pitch & readback, law params or table)

        private const int CONTROL_OFFSET = 0;
        private const int CONTROL_F = 0;
        private const int STATUS_OFFSET = 0;
        private const int STATUS_F = 1;
        private const int CURRENT_MASS_OFFSET = 0;
        private const int CURRENT_MASS_F = 2;
        private const int NUM_STAGES_OFFSET = 1;
        private const int NUM_STAGES_F = 0;
        private const int ACTIVE_STAGE_OFFSET = 1;
        private const int ACTIVE_STAGE_F = 1;
        private const int THROTTLE_OFFSET = 1;
        private const int THROTTLE_F = 2;

        /// Offsets for stage data within a stage
        private const int MASS_OFFSET = 0;
        private const int MASS_DRY_F = 0;
        private const int MASS_FUEL_F = 1;
        private const int MASS_PAYLOAD_F = 2;
        private const int THRUST_OFFSET = 1;
        private const int THRUST_F = 0;
        private const int FUEL_RATE_OFFSET = 1;
        private const int FUEL_RATE_F = 1;
        private const int ISP_OFFSET = 2;
        private const int ISP_F = 2;

        // Offsets for steering info from steering base
        private const int GUID_LAW_OFFSET = 0;
        private const int GUID_LAW_F = 0;
        private const int GUID_TSTART_OFFSET = 0;
        private const int GUID_TSTART_F = 1;
        private const int GUID_TEND_OFFSET = 0;
        private const int GUID_TEND_F = 2;
        private const int GUID_CENTER_OFFSET = 1;
        private const int GUID_PLANE_OFFSET = 2;
        private const int GUID_PTABLE_OFFSET = 3;
        private const int GUID_PITCH_OFFSET = 3;
        private const int PITCH_F = 2;
        /// <summary>
		/// Utility class to bundle together the attributes for a rocket stage. Some redundancy
		/// (payload and dry mass).
		///
		/// Stage has a couple of helper methods that do rough calculations to determine what dV
		/// can be attained using rocket eqn, gravity loss.
		/// 
		/// </summary>
        public class Stage {
            // masses in kg, thrust in N
            public double mass_payload;
            public double mass_dry;
            public double mass_fuel;
            public double thrustN;
            // burn time assuming 100% thrust. Used to determine the fuel_rate
            public double burn_time_sec;
        }


        /// <summary>
        /// Create data[] array for a linear tangent steering law of the form tan(theta) = (1-t/t_f) tan(theta0)
        /// as found in (6.59) of "Design of Rockets and Launch Vehicles", Edberg & Cpsta, AIAA Press.
        ///
        /// This uses a standard rocket equation data block but note that if the engine is throttled down the algorithm
        /// will still assume the same t_burn and compute the guidance accordingly.
        ///
        /// SB = Steering Base
		/// [SB0] (law, t_start, t_end)     start/end are for overall flight path for steering laws that use this
        /// [SB1] center
        /// [SB2] plane
		/// [SB3] (tan(theta0), 0, pitch_readback)
        ///
        /// </summary>
        /// <param name="theta0Deg"></param>
        /// <param name="t_final"></param>
        /// <returns></returns>
        public static double3[] LinearTangentECAlloc(double theta0Deg, int numStages)
        {
            double3[] data = new double3[HEADER_SIZE + numStages * STAGE_SIZE + GUID_SIZE + 1];
            data[0] = new double3(0, 0, 0);
            data[1] = new double3(numStages, 0, 0);
            data[HEADER_SIZE + numStages * STAGE_SIZE] = new double3((int)Guidance.LINEAR_TANGENT_EC, 0, 0);
            data[HEADER_SIZE + numStages * STAGE_SIZE + GUID_SIZE] = new double3(math.tan(math.radians(theta0Deg)), 0, 0);
            return data;
        }

        /// <summary>
        /// Set the ENABLED flag in the data block. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="enabled"></param>
        public static void EnabledSet(double3[] data, bool enabled)
        {
            if (enabled)
                data[0].x = (int)data[0].x | ENABLED;
            else
                data[0].x = (int)data[0].x & ~ENABLED;
        }

        /// <summary>
        /// Set the auto-stage control bit in the booster data block to 
        /// indicated the value of the enabled parameter.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="enabled"></param>
        public static void AutoStageSet(double3[] data, bool enabled)
        {
            if (enabled)
                data[CONTROL_OFFSET][CONTROL_F] = (int)data[CONTROL_OFFSET][CONTROL_F] | AUTO_STAGE;
            else
                data[CONTROL_OFFSET][CONTROL_F] = (int)data[CONTROL_OFFSET][CONTROL_F] & ~AUTO_STAGE;
        }

        /// <summary>
        /// Allocate a data block with numStages that will use a manual pitch value
        /// for ascent guidance. 
        /// 
        /// This adds a single double3 to the standard header + stages that contains:
        /// (SE = end of stage block + 1)
        /// data[SE+0].x = manual pitch value 
        /// </summary>
        /// <param name="pitchRad"></param>
        /// <param name="numStages"></param>
        /// <returns></returns>
        public static double3[] ManualPitchAlloc(double pitchRad, int numStages)
        {
            double3[] data = new double3[HEADER_SIZE + numStages * STAGE_SIZE + GUID_SIZE + 1];
            data[0] = new double3(0, 0, 0);
            data[1] = new double3(numStages, 0, 0);
            data[HEADER_SIZE + numStages * STAGE_SIZE] = new double3((int)Guidance.MANUAL_PITCH, 0, 0);
            data[HEADER_SIZE + numStages * STAGE_SIZE + GUID_SIZE] = new double3(pitchRad, 0, 0);
            return data;
        }

        public static void ManualPitchUpdate(double pitchRad, double3[] data)
        {
            int n = (int)data[NUM_STAGES_OFFSET][NUM_STAGES_F];
            data[HEADER_SIZE + n * STAGE_SIZE + GUID_SIZE + GUID_PITCH_OFFSET][PITCH_F] = pitchRad;
        }


        /// <summary>
		/// Full manual control with acceleration explicitly specified.
		///
		/// Atypical since does not have any stages or standard steering plane info.
        /// [SB0] (law, 0, 0)
        /// [SB1] thrustDir
		/// 
		/// </summary>
		/// <param name="thrustDir">thrust direction vector (normalized)</param>
		/// <returns></returns>
        public static double3[] ManualVecAlloc(double3 thrustDir, int numStages)
        {
            double3[] data = new double3[HEADER_SIZE + numStages * STAGE_SIZE + GUID_SIZE + 2];
            data[0] = new double3(0, 0, 0);
            data[1] = new double3(numStages, 0, 0);
            int guid_base = HEADER_SIZE + numStages * STAGE_SIZE;
            data[guid_base] = new double3((int)Guidance.MANUAL_VEC, 0, 0);
            data[guid_base + GUID_SIZE] = math.normalize(thrustDir);
            return data;
        }

        public static void ManualVecSetThrustDir(double3[] data, double3 thrustDir)
        {
            int numStages = (int)data[1].x;
            data[HEADER_SIZE + numStages * STAGE_SIZE + 1] = math.normalize(thrustDir);
        }

        /// <summary>
        /// Configure the PEG2D steering law.
        /// 
        /// mode:
        /// GRAVITY_TURN:
        /// The first stage will use gravity turn to target a circular orbit at the specified altitude.
        /// 
        /// The second stage will use PEG to target a circular orbit at the specified altitude.
        /// 
        /// PEG_AT_START:
        /// The first stage will use PEG to target a circular orbit at the specified altitude.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="steering_base"></param>
        /// <returns></returns>
        /// [G0] = (mode, gt_at_vel, gt_pitch_kick)
        /// [G1] = (pitch_rate, target_altitude, target_vz)
        /// [G2] = (isp, cycle_time, 0)
        /// [G3] = (peg_a, peg_b, peg_c)
        /// [G4] = (peg_t, peg_lastCall, 0)
        /// mode = 1: gravity turn
        /// mode = 2: PEG_AT_START
        /// gt_at_vel = velocity at which gravity turn is initiated
        /// gt_pitch_kick = pitch kick angle when gravity turn is initiated
        /// pitch_rate = rate of change of pitch angle
        /// target_altitude = target altitude of orbit (m)
        /// cycle_time = interval at which PEG is applied
        /// peg_a, peg_b, peg_c, peg_t = coefficients for the PEG equation
        /// target_vz = target vertical velocity (m/s)      

        private const int GT_STAGE_0 = 1 << 0;
        private const int PEG_START = 1 << 1;
        private const int PEG2D_MODE_F = 0;

        private const int PEG2D_MODE_OFFSET = 0;
        private const int PEG2D_GT_AT_VEL_OFFSET = 0;
        private const int PEG2D_GT_AT_VEL_F = 1;
        private const int PEG2D_GT_PITCH_KICK_OFFSET = 0;
        private const int PEG2D_GT_PITCH_KICK_F = 2;
        private const int PEG2D_PITCH_RATE_OFFSET = 1;
        private const int PEG2D_PITCH_RATE_F = 0;
        private const int PEG2D_TARGET_ALTITUDE_OFFSET = 2;
        private const int PEG2D_TARGET_ALTITUDE_F = 1;

        private const int PEG2D_CYCLE_TIME_OFFSET = 1;
        private const int PEG2D_CYCLE_TIME_F = 2;
        private const int PEG2D_PEG_A_OFFSET = 3;
        private const int PEG2D_PEG_A_F = 0;
        private const int PEG2D_PEG_B_OFFSET = 3;
        private const int PEG2D_PEG_B_F = 1;
        private const int PEG2D_PEG_C_OFFSET = 3;
        private const int PEG2D_PEG_C_F = 2;
        private const int PEG2D_PEG_T_OFFSET = 4;
        private const int PEG2D_PEG_T_F = 0;
        private const int PEG2D_TARGET_VZ_OFFSET = 1;
        private const int PEG2D_TARGET_VZ_F = 2;
        private const int PEG2D_PEG_LAST_CALL_OFFSET = 4;
        private const int PEG2D_PEG_LAST_CALL_F = 1;
        private const int PEG_GUID_SIZE = 5;
        public enum PEG2DMode {
            GRAVITY_TURN_S0,
            PEG_AT_START
        }
        public static double3[] Peg2DAlloc(int numStages,
                                        PEG2DMode mode,
                                        double gt_at_vel,
                                        double gt_pitch_kick,
                                        double pitch_rate,
                                        double target_altitude,
                                        double target_vz,
                                        double cycle_time
                                        )
        {
            double3[] data = new double3[HEADER_SIZE + numStages * STAGE_SIZE + PEG_GUID_SIZE + 1];
            data[0] = new double3(0, 0, 0);
            data[1] = new double3(numStages, 0, 0);

            int guid_base = HEADER_SIZE + numStages * STAGE_SIZE;
            Guidance gmode = Guidance.PEG_2D;
            if (mode == PEG2DMode.GRAVITY_TURN_S0) {
                gmode = Guidance.PEG2D_GT;
            }
            data[guid_base + PEG2D_MODE_OFFSET] = new double3((int)gmode, gt_at_vel, gt_pitch_kick);
            data[guid_base + PEG2D_TARGET_ALTITUDE_OFFSET] = new double3(pitch_rate, target_altitude, cycle_time);
            data[guid_base + PEG2D_PEG_A_OFFSET] = new double3(0, 0, 0);
            data[guid_base + PEG2D_PEG_T_OFFSET] = new double3(0, target_vz, 0);
            return data;
        }

        /// <summary>
        /// Add a stage to the Booster. This assumes data[] has been created
        /// with the correct number of total stages, prior to using this
        /// method to fill in the details. 
        /// 
        /// The mass of the payload should include the mass of all higher stages + fuel. 
        /// This can be calculated automatically by using the utility PayloadCompute().
        /// 
        /// </summary>
        /// <param name="data">data block for the booster</param>
        /// <param name="stageNum">number of this stage (0 based)</param>
        /// <param name="stage">struct with details of this stage</param>
        /// <param name="throttle">throttle value</param>
        public static void StageSetup(double3[] data,
                            int stageNum,
                            Stage stage)
        {
            int b = HEADER_SIZE + stageNum * STAGE_SIZE;
            double fuel_rate = stage.mass_fuel / stage.burn_time_sec;
            data[b + MASS_OFFSET] = new double3(stage.mass_dry, stage.mass_fuel, stage.mass_payload);
            double isp = stage.thrustN / (fuel_rate * GBUnits.g_earth);
            data[b + THRUST_OFFSET] = new double3(stage.thrustN, fuel_rate, isp);
        }

        /// <summary>
        /// Compute and configure the per stage payload values for each stage in a multi-stage booster. 
        /// e.g. 3 stages: 0, 1, 2
        /// Stage 2 has a payload mass of payloadMassKg. 
        /// Stage 1 has a payload mass of playloadMassKg + stage2 (fuel + dry mass)
        /// Stage 0 has a payload mass of stage 1 payload mass + stage1 (fuel + dry mass)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="payloadMassKg"></param>
        public static void PayloadCompute(double3[] data, double payloadMassKg)
        {
            int stageNum = (int)data[1].x;
            double stackMass = payloadMassKg;
            int stageMassIndex = HEADER_SIZE + STAGE_SIZE * (stageNum - 1);
            data[stageMassIndex].z = stackMass;
            stackMass += data[stageMassIndex].x + data[stageMassIndex].y; // dry + fuel
            while (--stageNum >= 0) {
                stageMassIndex -= STAGE_SIZE;
                data[stageMassIndex].z = stackMass;
                stackMass += data[stageMassIndex].x + data[stageMassIndex].y;
            }
        }

        public static double PayloadReadout(double3[] data, int stageNum)
        {
            return data[HEADER_SIZE + STAGE_SIZE * stageNum][MASS_PAYLOAD_F];
        }

        public static void ThrottleSet(double3[] data, double throttle)
        {
            data[THROTTLE_OFFSET][THROTTLE_F] = throttle;
        }


        public static void SteeringPlaneSet(double3[] data, double3 orbitCenter, double3 orbitNormal)
        {
            int n = (int)data[NUM_STAGES_OFFSET][NUM_STAGES_F];
            int guid_base = HEADER_SIZE + n * STAGE_SIZE;
            data[guid_base + GUID_CENTER_OFFSET] = orbitCenter;
            data[guid_base + GUID_PLANE_OFFSET] = orbitNormal;
        }

        public static void SteeringTimesSet(double3[] data, double tStartSec, double tEndSec)
        {
            int n = (int)data[NUM_STAGES_OFFSET][NUM_STAGES_F];
            int guid_base = HEADER_SIZE + n * STAGE_SIZE;
            data[guid_base + GUID_TSTART_OFFSET][GUID_TSTART_F] = tStartSec;
            data[guid_base + GUID_TEND_OFFSET][GUID_TEND_F] = tEndSec;
        }

        /// <summary>
		/// Get fuel for the designated stage. If the stage is -1, this is a request for the active stage.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="stage">0 based stage number</param>
		/// <returns></returns>
        public static double FuelReadout(double3[] data, int stageReq = -1)
        {
            int stage = stageReq;
            if (stageReq < 0)
                stage = (int)data[ACTIVE_STAGE_OFFSET][ACTIVE_STAGE_F];
            int n = (int)data[NUM_STAGES_OFFSET][NUM_STAGES_F];
            double fuel = -1;
            if (stage < n)
                fuel = data[HEADER_SIZE + stage * STAGE_SIZE + MASS_OFFSET][MASS_FUEL_F];
            return fuel;
        }

        // PitchTable with Rocket Equation
        // [SB0] (law, t_start, t_end)
        // [SB1] center
        // [SB2] plane
        // [SB3] Table entry 0 .z is num entries
        // [SB4] Table entry 1 .z is last index
        // [SB5] Table entry 1 .z is pitch readback
        // ...
        public static double3[] PitchTableAlloc(double3[] pitchTable, int numStages)
        {
            double3[] data = new double3[HEADER_SIZE + numStages * STAGE_SIZE + GUID_PTABLE_OFFSET + pitchTable.Length];
            data[0] = new double3(0, 0, 0);
            data[1] = new double3(numStages, 0, 0);
            int b = HEADER_SIZE + numStages * STAGE_SIZE;
            data[b] = new double3((int)Guidance.PITCH_TABLE, 0, 0);
            b += GUID_PTABLE_OFFSET;
            for (int i = 0; i < pitchTable.Length; i++)
                data[i + b] = pitchTable[i];
            data[b].z = pitchTable.Length;
            data[b + 1].z = 1; // point to 2nd entry to kick off
            return data;
        }

        public static double PitchReadback(double3[] data)
        {
            int n = (int)data[NUM_STAGES_OFFSET][NUM_STAGES_F];
            int guid_base = HEADER_SIZE + n * STAGE_SIZE;
            Guidance steering = (Guidance)data[guid_base].x;
            switch (steering) {
                case Guidance.LINEAR_TANGENT_EC:
                    return data[guid_base + GUID_PITCH_OFFSET][PITCH_F];

                case Guidance.PITCH_TABLE:
                    return data[guid_base + 5].z;
            }
            return 0;
        }

        public static int ActiveStageReadback(double3[] data)
        {
            return (int)data[1].y;
        }

        /// <summary>
        /// Static method to run the thrust and do steering calculations for a multi-stage
        /// booster. The operational parameters for the engine are contained in the the
        /// data[] block. This is interpreted based on the type of booster and steering law
        /// configured when initializing the data block. 
        /// 
        /// *** MAIN ENTRY POINT WHEN RUNNING AS PART OF GEPhysicsCore ***
        /// 
        /// </summary>
        /// <param name="eaState"></param>
        /// <param name="eaDesc"></param>
        /// <param name="t"></param>
        /// <param name="dt"></param>
        /// <param name="a_in"></param>
        /// <param name="data"></param>
        /// <param name="a_out"></param>
        /// <returns></returns>
        public static int ThrustAndSteering(ref ExternalAccel.EAStateData eaState,
                                ref ExternalAccel.EADesc eaDesc,
                                double t,
                                double dt,
                                double3 a_in,
                                NativeArray<double3> data,
                                out double3 a_out,
                                double scaleTtoSec,
                                double scaleAccelSitoGE)
        {
            int status = 0;
            a_out = double3.zero;
            int b = eaDesc.paramBase;
            // if not enabled bail out
            if (((int)data[b].x & ENABLED) == 0) {
                a_out = double3.zero;
                return status;
            }
            double3 r_from = eaState.r_from;
            // registered fuel out already
            bool fuel_out = ((int)data[b][STATUS_F] & FUEL_OUT) != 0;
            if (fuel_out) {
                a_out = double3.zero;
                return status;
            }
            int active_stage = (int)data[b + ACTIVE_STAGE_OFFSET][ACTIVE_STAGE_F];
            int stage_base = b + HEADER_SIZE + active_stage * STAGE_SIZE;
            int numStages = (int)data[b + NUM_STAGES_OFFSET][NUM_STAGES_F];
            int steer_base = b + HEADER_SIZE + numStages * STAGE_SIZE;

            // determine the magnitude of the acceleration change. 
            double tSec = scaleTtoSec * t;
            double dtSec = scaleTtoSec * dt;
            double accelSI = RocketEqn(data, tSec, dtSec, stage_base, b);
            // convert m/sec to GECore accel scale
            double accelGE = accelSI * scaleAccelSitoGE;

            Guidance law = (Guidance)data[steer_base][GUID_LAW_F];
            switch (law) {
                case Guidance.MANUAL_VEC: {
                        double3 thrustDir = data[steer_base + 1];
                        a_out = accelSI * thrustDir;
                    }
                    break;

                case Guidance.LINEAR_TANGENT_EC: {
                        double t_rel = tSec - data[steer_base].y;
                        double t_burn = data[steer_base].z - data[steer_base].y;
                        if (t_rel > t_burn)
                            t_rel = t_burn;
                        double pitch = math.atan((1.0 - t_rel / t_burn) * data[steer_base + 3].x);
                        double3 tmp = data[steer_base + 3];
                        tmp.z = pitch;
                        data[steer_base + 3] = tmp;
                        double3 thrustDir = PitchSteering(data, r_from, steer_base, pitch);
                        a_out = accelGE * thrustDir;
                    }
                    break;

                case Guidance.MANUAL_PITCH: {
                        double pitch = data[steer_base + 3].x;  // manual pitch in radians
                        double3 thrustDir = PitchSteering(data, r_from, steer_base, pitch);
                        a_out = accelGE * thrustDir;
                    }
                    break;

                case Guidance.PITCH_TABLE: {
                        double pitch = PitchFromTable(data, tSec, steer_base);
                        double3 thrustDir = PitchSteering(data, r_from, steer_base, pitch);
                        a_out = accelGE * thrustDir;
                    }
                    break;

                case Guidance.PEG_2D: {
                        double muSI = GBUnits.moonMu;
                        double isp = data[stage_base + ISP_OFFSET][ISP_F];
                        double pitch = Peg2DSteering(data, steer_base, r_from, eaState.v_from, muSI, dtSec, accelSI, isp);
                        double3 thrustDir = PitchSteering(data, r_from, steer_base, pitch);
                        a_out = accelGE * thrustDir;
                    }
                    break;

            }
            // did we stage?
            int stage_now = (int)data[b + ACTIVE_STAGE_OFFSET][ACTIVE_STAGE_F];
            // did we run out of fuel this call? 
            fuel_out = ((int)data[b][STATUS_F] & FUEL_OUT) != 0;
            if (fuel_out) {
                status = STATUS_FUEL_OUT;
            } else if (stage_now != active_stage) {
                status = STATUS_STAGED;
            }
            return status;
        }

        /// <summary>
        /// Determine the magnitude of the acceleration based on the current booster state
        /// 
        /// Fuel rate is expressed in kg/sec and tmie 
        /// </summary>
        /// <param name="data">data block for boostewr</param>
        /// <param name="t">current time in seconds</param>
        /// <param name="dt">time delta in seconds</param>
        /// <param name="stage_base">base index for stage 0</param>
        /// <param name="b">index into the ENTIRE data array of ext accel data</param>
        /// <returns></returns>
        private static double RocketEqn(NativeArray<double3> data, double t, double dt, int stage_base, int b)
        {
            // determine magnitude of acceleration
            double thrustPercent = data[b + 1].z;
            double fuelRate = data[stage_base + FUEL_RATE_OFFSET][FUEL_RATE_F] * thrustPercent;
            double massFuel = data[stage_base + MASS_OFFSET][MASS_FUEL_F] - fuelRate * dt;
            // determine if fuel is out, or if we need to stage
            // some imprecision here since burn full size dt chunk when fuel is out...
            double3 tmp = data[b];
            if (massFuel <= 0) {
                massFuel = 0;
                int stage = (int)data[b + 1].y;
                int numStages = (int)data[b + 1].x;
                if (stage >= numStages - 1) {
                    tmp.x = (int)data[b].x & ~ENABLED;  // update control
                    tmp.y = (int)data[b].y | FUEL_OUT;  // update status
                    tmp.z = data[b].z;
                    data[b] = tmp;
                    // fuel out
                    // disable the external acceleration
                } else {
                    // advance to next stage
                    tmp.x = data[b + 1].x;  // numStages
                    tmp.y = stage + 1;      // active stage
                    tmp.z = data[b + 1].z;  // reserved
                    data[b + 1] = tmp;
                }
            }
            tmp = data[stage_base + MASS_OFFSET];
            tmp.y = massFuel;   // copy back the current amount of fuel
            data[stage_base + MASS_OFFSET] = tmp;

            // compute acceleration. Add dry_mass, fuel, payload
            double mass = data[stage_base + MASS_OFFSET][MASS_DRY_F] + massFuel + data[stage_base + MASS_OFFSET][MASS_PAYLOAD_F];
            // update total mass entry
            tmp = data[b];
            tmp.z = mass;
            data[b] = tmp;
            double thrust = data[stage_base + THRUST_OFFSET][THRUST_F] * thrustPercent;
            // a = F/m
            return thrust / mass;
        }

        /// <summary>
        /// Apply pitch as angle from the local horizontal in the orbital plane. 
        /// (This will differ slightly from the "flat earth" assumption in most 
        /// </summary>
        /// <param name="rNormal">direction from planet center to booster</param>
        /// <param name="orbitNormal">normal vector to launch plane</param>
        /// <param name="pitchRad">angle from local horizonatal (rad.)</param>
        /// <returns></returns>
        private static double3 PitchSteering(NativeArray<double3> data, double3 r_from, int steering_base, double pitchRad)
        {
            double3 origin = data[steering_base + GUID_CENTER_OFFSET];
            double3 rNormal = math.normalize(r_from - origin);
            double3 orbitNormal = data[steering_base + GUID_PLANE_OFFSET];
            // pitch is measured from the horizontal. This is computed from cross of N and R
            double3 r_h = math.normalize(math.cross(orbitNormal, rNormal));
            return math.cos(pitchRad) * r_h + math.sin(pitchRad) * rNormal;
        }

        /// <summary>
        /// Look up the entry in the pitch table. Assume the pitch table might not be regular and that since we're
        /// integrating time will advance forward monotonically. Just walk the table to find an entry but keep track of
        /// where we were. 
        /// 
        /// (Public so Unit tests can find it)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="t"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double PitchFromTable(NativeArray<double3> data, double t, int steering_base)
        {
            double t_start = data[steering_base].y;
            double tRel = t - t_start;
            int pb = steering_base + GUID_PTABLE_OFFSET;
            // cache the last used index in z component of the 1st entry
            int i = (int)data[pb + 1].z;
            int num = (int)data[pb].z;
            double pitch;
            double3 tmp;
            if (i < num - 2) {
                // has time moved on to next point?
                while ((tRel > data[pb + i + 1].x) && (i < num - 1)) {
                    i += 1;
                }
                double t01 = (tRel - data[pb + i - 1].x) / (data[pb + i].x - data[pb + i - 1].x);
                // have t <= t[i]. Just do linear interpolation
                pitch = math.lerp(data[pb + i - 1].y, data[pb + i].y, t01);
                // store last index used
                tmp = data[pb + 1];
                tmp.z = i;
                data[pb + 1] = tmp;
            } else {
                pitch = data[pb + num - 1].y;
            }
            // store last pitch
            tmp = data[pb + 2];
            tmp.z = pitch; // store last pitch (radians)
            data[pb + 2] = tmp;
            return pitch;
        }


        private static double Peg2DSteering(NativeArray<double3> data, int steering_base, double3 r, double3 v, double mu, double dt, double thrust, double isp)
        {
            PEG2DMode mode = (PEG2DMode)data[steering_base + PEG2D_MODE_OFFSET][PEG2D_MODE_F];
            if (mode == PEG2DMode.GRAVITY_TURN_S0) {
                return 0;
            }
            double3 orbitNormal = data[steering_base + GUID_PLANE_OFFSET];
            double3 r_unit = math.normalize(r);
            double3 v_unit = math.cross(orbitNormal, r_unit);
            // Set PEG altitude to current radial distance from center of earth [m]
            double PEG_alt = math.length(r);
            // Current tangential velocity [m/s]
            double PEG_vt = math.dot(v, v_unit);
            // Current radial velocity (in z-direction) [m/s]
            double PEG_vr = math.dot(v, r_unit);
            // Effective exhaust velocity [m/s]

            double PEG_ve = isp * GBUnits.g_earth;
            // Current acceleration [m/s2]
            double PEG_acc = thrust;

            double PEG_A = data[steering_base + PEG2D_PEG_A_OFFSET][PEG2D_PEG_A_F];
            double PEG_B = data[steering_base + PEG2D_PEG_B_OFFSET][PEG2D_PEG_B_F];
            double PEG_C = data[steering_base + PEG2D_PEG_C_OFFSET][PEG2D_PEG_C_F];
            double PEG_T = data[steering_base + PEG2D_PEG_T_OFFSET][PEG2D_PEG_T_F];
            double targetVz = data[steering_base + PEG2D_TARGET_VZ_OFFSET][PEG2D_TARGET_VZ_F];
            double PEG_lastCall = data[steering_base + PEG2D_PEG_LAST_CALL_OFFSET][PEG2D_PEG_LAST_CALL_F];

            double targetAltitude = data[steering_base + PEG2D_TARGET_ALTITUDE_OFFSET][PEG2D_TARGET_ALTITUDE_F];
            double cycleTime = data[steering_base + PEG2D_CYCLE_TIME_OFFSET][PEG2D_CYCLE_TIME_F];


            // To save computation time, run major loop only every cycleTime seconds
            // To know the time passed, this function stores the burnout time T to PEG_lastCall
            if ((PEG_lastCall - PEG_T) >= cycleTime) {
                (PEG_A, PEG_B, PEG_C, PEG_T) = PoweredExplicitGuidance.CalculateABCT(
                    cycleTime, mu, PEG_alt, PEG_vt, PEG_vr,
                    targetAltitude, PEG_acc, PEG_ve, PEG_A, PEG_B, PEG_T);

                PEG_lastCall = PEG_T;
            } else {
                PEG_T -= dt;
            }
            // Calculate pitch
            double sinPitch = PEG_A - (PEG_lastCall - PEG_T) * PEG_B + PEG_C;
            double pitch = math.degrees(math.asin(math.clamp(sinPitch, -1f, 1f)));
            UnityEngine.Debug.Log($"PEG_A: {PEG_A}, PEG_B: {PEG_B}, PEG_C: {PEG_C}, PEG_T: {PEG_T}, PEG_lastCall: {PEG_lastCall}, PEG_T: {PEG_T}, pitch: {pitch}");

            // retain PEG data in the data array
            data[steering_base + PEG2D_PEG_A_OFFSET] = new double3(PEG_A, PEG_B, PEG_C);
            data[steering_base + PEG2D_PEG_T_OFFSET] = new double3(PEG_T, PEG_lastCall, 0);
            return pitch;
        }

        public static (int stage, double fuel_used) StageNumFuelAtTime(double3[] data, double t)
        {
            int numStages = (int)data[1].x;
            int stage = 0;
            double t_net = t;
            while (stage < numStages) {
                int stageIndex = HEADER_SIZE + STAGE_SIZE * stage;
                double burn_time = data[stageIndex].y / data[stageIndex + 1].y;
                if (t_net < burn_time) {
                    return (stage, t_net / burn_time * data[stageIndex].y);
                }
                t_net -= burn_time;
                stage++;
            }
            return (stage, 0);
        }

        // The following methods are used during trajectory preview. They assume that the data[] block is
        // configured for the initial configuration of the booster and has not been changed.
        public static double MassAtTime(double3[] data, double t)
        {
            (int stage, double fuel_used) = StageNumFuelAtTime(data, t);
            int numStages = (int)data[1].x;
            if (stage >= numStages) {
                return data[HEADER_SIZE + STAGE_SIZE * (numStages - 1)].z;
            }
            int stageMassIndex = HEADER_SIZE + STAGE_SIZE * stage;
            double payloadMass = data[stageMassIndex].z; // payload mass is everything above this stage
            payloadMass += data[stageMassIndex].x; // add dry mass
            payloadMass += data[stageMassIndex].y; // add fuel mass
            payloadMass -= fuel_used; // fuel
            return payloadMass;
        }

        public static double ThrustAtTime(double3[] data, double t)
        {
            (int stage, double fuel_used) = StageNumFuelAtTime(data, t);
            int stageThrustIndex = HEADER_SIZE + STAGE_SIZE * stage;
            return data[stageThrustIndex + 1].x;
        }

        public static double BurnTimeForStage(double3[] data, int stage)
        {
            int stageIndex = HEADER_SIZE + STAGE_SIZE * stage;
            return data[stageIndex].y / data[stageIndex + 1].y;
        }

        // TODO: REFACTOR and get rid of NativeArray version??

        /// <summary>
        /// Total burn time for the booster extracted from a copy of the data[] block
        /// with 0 offset.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double TotalBurnTime(double3[] data)
        {
            int numStages = (int)data[1].x;
            double totalBurnTime = 0;
            for (int i = 0; i < numStages; i++) {
                int index = HEADER_SIZE + STAGE_SIZE * i;
                totalBurnTime += data[index].y / data[index + 1].y; // fuel/fuel_rate
            }
            return totalBurnTime;
        }


#if JUNK
        public static void PEGStep(double3[] data, int guid_base, double dt)
        {
            double cycleTime = data[guid_base + PEG2D_CYCLE_TIME_OFFSET][PEG2D_CYCLE_TIME_F];
            double targetAltitude = data[guid_base + PEG2D_TARGET_ALTITUDE_OFFSET][PEG2D_TARGET_ALTITUDE_F];
            double targetVz = data[guid_base + PEG2D_TARGET_VZ_OFFSET][PEG2D_TARGET_VZ_F];

            // Set PEG altitude to current radial distance from center of earth [m]
            double PEG_alt = R + Altitude;
            // Current tangential velocity [m/s]
            double PEG_vt = vel.x + velXGain;
            // Current radial velocity (in z-direction) [m/s]
            double PEG_vr = vel.z;
            // Effective exhaust velocity [m/s]
            double PEG_ve = Isp * g0;
            // Current acceleration [m/s2]
            double PEG_a = PEG_ve * MassFlowRate / Mass;

            // To save computation time, run major loop only every cycleTime seconds
            // To know the time passed, this function stores the burnout time T to PEG_lastCall
            if ((PEG_lastCall - PEG_T) >= cycleTime) {
                (PEG_A, PEG_B, PEG_C, PEG_T) = PoweredExplicitGuidance.Calculate(
                    cycleTime, mu, PEG_alt, PEG_vt, PEG_vr,
                    targetAltitude, PEG_a, PEG_ve, PEG_A, PEG_B, PEG_T);

                PEG_lastCall = PEG_T;
            } else {
                PEG_T -= dt;
            }

            // Calculate pitch
            double sinPitch = PEG_A - (PEG_lastCall - PEG_T) * PEG_B + PEG_C;
            double pitch = math.degrees(math.asin(math.clamp(sinPitch, -1f, 1f)));
            this.pitch = pitch;

            // Perform rocket physics step
            // Vessel mass changes
            double m = Mass;
            double dm = dt * MassFlowRate;
            Mass = m - dm;

            // Calculate gravity and centrifugal force
            double ca = (vel.x + velXGain) * (vel.x + velXGain) / radius;
            double ga = mu / (radius * radius);
            double vg = (ca - ga) * dt;

            // Update velocities
            double pitchRad = math.radians(this.pitch);
            vel = new double3(
                vel.x + PEG_a * math.cos(pitchRad) * dt,
                vel.y,
                vel.z + vg + PEG_a * math.sin(pitchRad) * dt
            );
            pos += vel * dt;

            // Update position
            Altitude = pos.z;
            radius = Altitude + R;

            // Update time
            time += dt;
        }
#endif
    }
}
