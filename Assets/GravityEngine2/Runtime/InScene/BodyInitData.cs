using Unity.Mathematics;

namespace GravityEngine2 {
    /// <summary>
    /// Container class to hold common info for init of an orbital body.
    ///
    /// Currently used in GSBody and GSDisplayOrbit (when want to show an orbit without a link to
    /// an existing body)
    /// 
    /// </summary>
    [System.Serializable]
    public class BodyInitData {
        public GBUnits.Units units;

        // R, V initial data (may be absolute or relative per initDataType)
        // double3 is a struct and has no serializer, so do a component at a time
        public double3 r;
        public double3 v;

        // Editor script does modal prompting in the inspector. Not all fields used in all cases!
        public enum InitDataType {
            RV_ABSOLUTE, RV_RELATIVE,
            COE, COE_ApoPeri, COE_HYPERBOLA, TWO_LINE_ELEMENT, LATLONG_POS
        };
        public InitDataType initData = InitDataType.RV_ABSOLUTE;

        // LATLONG_POS

        public enum LatLongType { CUSTOM, EARTH_SURFACE };
        public LatLongType latLongType = LatLongType.CUSTOM;
        public EarthLaunchSite.Site site;

        public bool earthRotation = true;
        public double latitude;
        public double longitude;

        public double altitude;

        // COE
        public double a = 10.0;
        public double eccentricity = 0.0;
        public double inclination = 0.0;
        public double omega_lc = 0.0;
        public double omega_uc = 0.0;
        public double nu = 0.0;
        // COE alt
        public double apoapsis = 10.0;
        public double periapsis = 10.0;
        public double p_semi = 10.0;     // semi-parameter (NOT periapsis!)
        // TLE (all three lines in one string)
        public string tleData = null;

        // for COE info of real-world bodies want to know when the COE was defined
        // (SolarSystemBuilder will fill this in)
        // Data from a TLE is also placed here. 
        public double startEpochJD = 0.0;

        public bool haveSatData;
        public SGP4SatData satData;

        public bool IsRVType()
        {
            return (initData == InitDataType.RV_ABSOLUTE) || (initData == InitDataType.RV_RELATIVE);
        }

        public bool IsCOEType()
        {
            return (initData == InitDataType.COE) || (initData == InitDataType.COE_ApoPeri)
                   || (initData == InitDataType.COE_HYPERBOLA);

        }

        /// <summary>
        /// Fill in the provided COE with the orbital elements in the units defined by the BID. 
        /// 
        /// </summary>
        /// <param name="coe"></param>
        /// <returns></returns>
        public bool FillInCOE(Orbital.COE coe)
        {
            if (initData == InitDataType.COE) {
                p_semi = a * (1 - eccentricity * eccentricity);
                coe.p = p_semi;
                coe.a = a;
            } else if (initData == InitDataType.COE_ApoPeri) {
                a = 0.5 * (apoapsis + periapsis);
                coe.a = 0.5 * (apoapsis + periapsis);
                // r_a = a(1+e)
                eccentricity = apoapsis / coe.a - 1.0;
                p_semi = a * (1 - eccentricity * eccentricity);
                coe.p = p_semi;
            } else if (initData == InitDataType.COE_HYPERBOLA) {
                coe.a = periapsis / (1.0 - eccentricity);   // will be negative since e > 1
                coe.p = coe.a * (1 - eccentricity * eccentricity); // +ve, since a < 0, e > 1
            } else if (initData == InitDataType.TWO_LINE_ELEMENT) {
                SGP4utils_GE2.TLEtoSatData(tleData, ref satData);
                if (satData.error != 0) {
                    UnityEngine.Debug.LogError("Could not init TLE data err=" +
                                                    SGP4SatData.ErrorString(satData.error));
                    return false;
                }
                satData.FillInCOEKmWithInitialData(ref coe);
                return true;
            } else {
                // UnityEngine.Debug.Log("Not implemented type="+initData);
                return false;
            }
            double D2R = GravityMath.DEG2RAD;
            if (eccentricity >= 1.0) {
                a *= -1.0;
            }
            coe.e = eccentricity;
            coe.i = inclination * D2R;
            coe.omegaL = omega_lc * D2R;
            coe.omegaU = omega_uc * D2R;
            coe.nu = nu * D2R;
            coe.ComputeRotation();
            coe.ComputeType();

            return true;
        }

#if UNITY_EDITOR
        public double SGP4StartEpoch()
        {
            if (initData == InitDataType.TWO_LINE_ELEMENT) {
                SGP4utils_GE2.TLEtoSatData(tleData, ref satData);
                haveSatData = true;
                return satData.jdsatepoch;
            }
            return 0;
        }


        public double EditorGetSize()
        {
            double size = 0;
            switch (initData) {
                case InitDataType.RV_RELATIVE:
                    size = math.length(r);
                    break;

                case InitDataType.COE:
                case InitDataType.COE_ApoPeri:
                    size = a;
                    break;

                case InitDataType.COE_HYPERBOLA:
                    size = periapsis;
                    break;

                case InitDataType.TWO_LINE_ELEMENT:
                    SGP4utils_GE2.TLEtoSatData(tleData, ref satData);
                    size = satData.a * GBUnits.earthRadiusKm;
                    haveSatData = true;
                    break;

                default:
                    break;
            }
            return size;
        }
#endif
    }
}
