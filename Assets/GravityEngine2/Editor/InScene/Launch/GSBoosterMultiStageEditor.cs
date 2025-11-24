using UnityEditor;
using UnityEngine;

namespace GravityEngine2
{

    [CustomEditor(typeof(GSBoosterMultiStage), true)]

    public class GSBoosterMultiStageEditor : Editor
    {

        double[] dryMassKg = new double[GSBoosterMultiStage.MAX_STAGES];
        double[] fuelMassKg = new double[GSBoosterMultiStage.MAX_STAGES];
        double[] thrustN = new double[GSBoosterMultiStage.MAX_STAGES];
        double[] burnTimeSec = new double[GSBoosterMultiStage.MAX_STAGES];

        double[] dragCoeff = new double[GSBoosterMultiStage.MAX_STAGES];
        double[] crossSectionalArea = new double[GSBoosterMultiStage.MAX_STAGES];


        public override void OnInspectorGUI()
        {
            GUI.changed = false;
            serializedObject.Update();

            GSBoosterMultiStage gsb = (GSBoosterMultiStage)target;

            EditorGUILayout.LabelField("SPACE to Launch, X to Preview", EditorStyles.boldLabel);

            GSBody payloadBody = (GSBody)EditorGUILayout.ObjectField("Payload Body", gsb.payloadBody,
                        typeof(GSBody), true);


            GSController gsc = (GSController)EditorGUILayout.ObjectField("GSController", gsb.gsController,
                        typeof(GSController), true);


            double displayOrbitAtHeightKm = EditorGUILayout.DoubleField("Display Orbit At Height (km)", gsb.displayOrbitAtHeightKm);
            double payloadMass = EditorGUILayout.DoubleField("Payload Mass (kg)", gsb.payloadMassKg);
            double payloadDragCoeff = EditorGUILayout.DoubleField("Payload Drag Coeff", gsb.payloadDragCoeff);
            double payloadCrossSectionalArea = EditorGUILayout.DoubleField("Payload Cross Sectional Area", gsb.payloadCrossSectionalArea);

            bool stagePayload = EditorGUILayout.Toggle("Stage Payload", gsb.stagePayload);

            int numStages = EditorGUILayout.IntField("Number of Stages", gsb.numStages);
            if (numStages > 4)
                numStages = 4;
            for (int i = 0; i < numStages; i++)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // horizontal line
                EditorGUILayout.LabelField("Stage " + i);
                dryMassKg[i] = EditorGUILayout.DoubleField("Dry mass (kg)", gsb.dryMassKg[i]);
                fuelMassKg[i] = EditorGUILayout.DoubleField("Fuel mass (kg)", gsb.fuelMassKg[i]);
                thrustN[i] = EditorGUILayout.DoubleField("Thrust (N)", gsb.thrustN[i]);
                burnTimeSec[i] = EditorGUILayout.DoubleField("Burn Time (sec.)", gsb.burnTimeSec[i]);
                dragCoeff[i] = EditorGUILayout.DoubleField("Drag Coeff", gsb.dragCoeff[i]);
                crossSectionalArea[i] = EditorGUILayout.DoubleField("Cross Sectional Area", gsb.crossSectionalArea[i]);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // horizontal line

            EditorGUILayout.LabelField("Guidance", EditorStyles.boldLabel);
            GSBoosterMultiStage.SteeringMode steerMode = (GSBoosterMultiStage.SteeringMode)
                EditorGUILayout.EnumPopup("Steering Mode", gsb.steeringMode);
            double steerParam = gsb.steeringParam;
            string pitchFile = gsb.pitchFile;
            double startTurnAtVelocitySI = gsb.startTurnAtVelocitySI;
            double startTurnPitchKickDeg = gsb.startTurnPitchKickDeg;
            double pitchRateDegPerSec = gsb.pitchRateDegPerSec;
            double targetAltitudeSI = gsb.targetAltitudeSI;
            bool stage1GravityTurn = gsb.stage1GravityTurn;
            switch (steerMode)
            {
                case GSBoosterMultiStage.SteeringMode.LINEAR_TAN_EC:
                    steerParam = EditorGUILayout.DoubleField("Steering Param", steerParam);
                    break;

                case GSBoosterMultiStage.SteeringMode.PITCH_TABLE:
                    pitchFile = EditorGUILayout.TextField("Pitch File", pitchFile);
                    break;

                    // case GSBoosterMultiStage.SteeringMode.PEG_2D:
                    //     targetAltitudeSI = EditorGUILayout.DoubleField("Target Altitude (m)", targetAltitudeSI);
                    //     stage1GravityTurn = EditorGUILayout.Toggle("Stage 1 Gravity Turn", stage1GravityTurn);
                    //     if (stage1GravityTurn) {
                    //         startTurnAtVelocitySI = EditorGUILayout.DoubleField("Start Turn At Velocity (m/s)", startTurnAtVelocitySI);
                    //         startTurnPitchKickDeg = EditorGUILayout.DoubleField("Start Turn Pitch Kick (deg)", startTurnPitchKickDeg);
                    //         pitchRateDegPerSec = EditorGUILayout.DoubleField("Pitch Rate (deg/s)", pitchRateDegPerSec);
                    //     }
                    //     break;
            }
            double targetInclDeg = EditorGUILayout.DoubleField("Target Incl. (deg)", gsb.targetInclDeg);

            bool earthAtmo = EditorGUILayout.Toggle("Earth Atmosphere", gsb.earthAtmosphere);

            TMPro.TMP_Text statusText = (TMPro.TMP_Text)EditorGUILayout.ObjectField("Status Text", gsb.statusText,
                        typeof(TMPro.TMP_Text), true);

            EditorGUILayout.LabelField("Manual Control via A/S keys", EditorStyles.boldLabel);
            bool manualKeys = EditorGUILayout.Toggle("Manual Key Control", gsb.manualKeys);

            if (GUI.changed)
            {
                Undo.RecordObject(gsb, "GSBoosterMultiStage");
                gsb.payloadBody = payloadBody;
                gsb.gsController = gsc;
                gsb.payloadMassKg = payloadMass;
                gsb.displayOrbitAtHeightKm = displayOrbitAtHeightKm;
                gsb.stagePayload = stagePayload;
                gsb.numStages = numStages;
                gsb.payloadDragCoeff = payloadDragCoeff;
                gsb.payloadCrossSectionalArea = payloadCrossSectionalArea;
                for (int i = 0; i < numStages; i++)
                {
                    gsb.dryMassKg[i] = dryMassKg[i];
                    gsb.fuelMassKg[i] = fuelMassKg[i];
                    gsb.thrustN[i] = thrustN[i];
                    gsb.burnTimeSec[i] = burnTimeSec[i];
                    gsb.dragCoeff[i] = dragCoeff[i];
                    gsb.crossSectionalArea[i] = crossSectionalArea[i];
                }
                gsb.steeringMode = steerMode;
                gsb.steeringParam = steerParam;
                gsb.pitchFile = pitchFile;
                gsb.targetInclDeg = targetInclDeg;
                gsb.startTurnAtVelocitySI = startTurnAtVelocitySI;
                gsb.startTurnPitchKickDeg = startTurnPitchKickDeg;
                gsb.pitchRateDegPerSec = pitchRateDegPerSec;
                gsb.stage1GravityTurn = stage1GravityTurn;
                gsb.targetAltitudeSI = targetAltitudeSI;
                gsb.earthAtmosphere = earthAtmo;
                gsb.manualKeys = manualKeys;
                gsb.statusText = statusText;
                EditorUtility.SetDirty(gsb);
            }
        }
    }
}
