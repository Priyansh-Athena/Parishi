using System;
using Unity.Mathematics;
using UnityEngine;

namespace GravityEngine2 {
    /// <summary>
	/// Class to load ephemeris data and place is GEBodyState array.
	///
	/// This array can then be used to init an EPHEMERIS propagator in GE.
	/// 
	/// </summary>
    public class EphemerisLoader {

        public static bool LoadFile(EphemerisData eData, GBUnits.Units worldUnits)
        {

            if (eData.fileName == null)
                Debug.DebugBreak();

            if (eData.fileName.EndsWith(".txt")) {
                Debug.LogError("Do not include .txt extension in name");
                return false;
            }

            TextAsset mytxtData = (TextAsset)Resources.Load(eData.fileName);
            if (mytxtData == null) {
                Debug.LogError("Could not access file: " + eData.fileName);
                return false;
            }
            string txt = mytxtData.text;

            string[] lines = txt.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            int numPoints = lines.Length;
            eData.data = new GEBodyState[numPoints];

            string[] splitLine;
            char[] delimiters = new char[] { ' ', '\t' };
            // Loop through the satellite lines
            for (int s = 0; s < numPoints; s++) {
                splitLine = lines[s].Trim().Split(delimiters);
                if (splitLine.Length != 7)
                    break;
                eData.data[s].t = I18N.DoubleParse(splitLine[0]);
                // TODO: Scale time
                eData.data[s].r = new double3(I18N.DoubleParse(splitLine[1]),
                    I18N.DoubleParse(splitLine[2]),
                    I18N.DoubleParse(splitLine[3]));
                eData.data[s].r *= GBUnits.DistanceConversion(eData.fileUnits, worldUnits);
                eData.data[s].v = new double3(I18N.DoubleParse(splitLine[4]),
                    I18N.DoubleParse(splitLine[5]),
                    I18N.DoubleParse(splitLine[6]));
                eData.data[s].v *= GBUnits.VelocityConversion(eData.fileUnits, worldUnits);
            }
            Debug.LogFormat("Read {0} points from {1}", numPoints, eData.fileName);
            return true;
        }

        /// <summary>
		/// Load a table of (t, pitch) with:
		///     t: world time from start of launch
		///     pitch: angle in radians from horizontal at time t
		///     
		/// </summary>
		/// <param name="resFile"></param>
		/// <returns>double3 array with x=t, y=pitch</returns>
        public static double3[] LoadPitchTable(string resFile)
        {
            TextAsset mytxtData = (TextAsset)Resources.Load(resFile);
            if (mytxtData == null) {
                Debug.LogError("Could not access file: " + resFile);
                return null;
            }
            string txt = mytxtData.text;

            string[] lines = txt.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            double3[] tPitch = new double3[lines.Length];
            string[] splitLine;
            char[] delimiters = new char[] { ' ', '\t', ',' };
            for (int i = 0; i < lines.Length; i++) {
                //Debug.Log("parse line:" + lines[i]);
                splitLine = lines[i].Trim().Split(delimiters);
                tPitch[i] = new double3(I18N.DoubleParse(splitLine[0]),
                                         I18N.DoubleParse(splitLine[1]),
                                         0);
            }
            return tPitch;
        }

    }
}
