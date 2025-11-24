using Unity.Collections;

namespace GravityEngine2 {
    /// <summary>
    /// Code used by GEPhysicsCore to evolve according to an ephemeris table. 
    /// The data values have been repackaged by GECore and converted into GE units
    /// by the time this code acts on them.
    /// </summary>
    public class EphemerisPropagator {
        public struct PropInfo {
            public int baseIndex;
            public int numPoints;
            public double tStart;
            // if there is no regular interval, then set this negative
            public double tInterval;
            // ephem data may be relative to some other object
            public bool relative;
            public int centerId;
        }

        public static void Evolve(double t,
                                  ref GEPhysicsCore.GEBodies bodies,
                                  ref NativeArray<PropInfo> propInfo,
                                  ref NativeArray<int> indices,
                                  int lenEphemBodies,
                                  ref NativeArray<GEBodyState> ephemerisData)
        {
            int pIndex;
            int tIndex;
            for (int j = 0; j < lenEphemBodies; j++) {
                int i = indices[j];
                pIndex = bodies.propIndex[i];
                if (propInfo[pIndex].tInterval > 0) {
                    tIndex = (int)((t - propInfo[pIndex].tStart) / propInfo[pIndex].tInterval);
                    // hold on last entry for now (could flip to Kepler if relative)
                    bool interpolate = true;
                    if (tIndex >= propInfo[pIndex].numPoints - 1) {
                        tIndex = propInfo[pIndex].numPoints - 1;
                        interpolate = false;
                    }
                    int bIndex = propInfo[pIndex].baseIndex;
                    // TODO: Better interpolation
                    if (interpolate) {
                        double f = (t - propInfo[pIndex].tStart - propInfo[pIndex].tInterval * tIndex) / propInfo[pIndex].tInterval;
                        bodies.r[i] = (1.0 - f) * ephemerisData[bIndex + tIndex].r +
                                        f * ephemerisData[bIndex + tIndex + 1].r;
                        bodies.v[i] = (1.0 - f) * ephemerisData[bIndex + tIndex].v +
                                        f * ephemerisData[bIndex + tIndex + 1].v;
                    } else {
                        bodies.r[i] = ephemerisData[bIndex + tIndex].r;
                        bodies.v[i] = ephemerisData[bIndex + tIndex].v;
                    }
                    if (propInfo[pIndex].relative) {
                        bodies.r[i] += bodies.r[propInfo[pIndex].centerId];
                        bodies.v[i] += bodies.v[propInfo[pIndex].centerId];
                    }

                }
            }
        }

        public static void EvolveRelative(double t,
                                          int propId,
                                          ref NativeArray<PropInfo> propInfo,
                                          ref NativeArray<GEBodyState> ephemerisData,
                                          ref GEBodyState state)
        {
            state.t = t;
            if (propInfo[propId].tInterval > 0) {
                int tIndex = (int)((t - propInfo[propId].tStart) / propInfo[propId].tInterval);
                if (tIndex < 0) {
                    tIndex = 0;
                }
                // hold on last entry for now (could flip to Kepler if relative)
                bool interpolate = true;
                if (tIndex > propInfo[propId].numPoints - 1) {
                    tIndex = propInfo[propId].numPoints - 1;
                    interpolate = false;
                }
                // TODO: Better interpolation
                int bIndex = propInfo[propId].baseIndex;
                if (interpolate) {
                    double f = (t - propInfo[propId].tStart - propInfo[propId].tInterval * tIndex) / propInfo[propId].tInterval;
                    state.r = (1.0 - f) * ephemerisData[bIndex + tIndex].r +
                                f * ephemerisData[bIndex + tIndex + 1].r;
                    state.v = (1.0 - f) * ephemerisData[bIndex + tIndex].v +
                                f * ephemerisData[bIndex + tIndex + 1].v;
                } else {
                    state.r = ephemerisData[bIndex + tIndex].r;
                    state.v = ephemerisData[bIndex + tIndex].v;
                }
            }
        }

        /// <summary>
		/// Create a PropInfo instance and fill it in.
		///
		/// Manage the one flat ephemData array used for all ephem propagators. This is sized on demand, so an
		/// add forces a grow and a copy.
		///
		/// Implementation assumes adding bodies with ephemeris data is not a common run-time activity.
		/// </summary>
		/// <param name="ephemData">Ephemeris data in world units</param>
		/// <param name="gePhysicsJob"></param>
		/// <param name="geScaler"></param>
		/// <param name="centerId"></param>
		/// <param name="pIndex"></param>
        public static void EphemDataAlloc(EphemerisData ephemData,
                                           ref GEPhysicsCore.GEPhysicsJob gePhysicsJob,
                                           GBUnits.GEScaler geScaler,
                                           int centerId,
                                           int pIndex)
        {
            int numPoints = ephemData.NumPoints();

            // Fill in prop info and copy raw data into "all in one" ephemData in GEPhysicsCore
            int size = gePhysicsJob.ephemerisData.Length;
            int newSize = size + numPoints;
            NativeArray<GEBodyState> oldEphem = gePhysicsJob.ephemerisData;
            gePhysicsJob.ephemerisData = new NativeArray<GEBodyState>(newSize, Allocator.Persistent);
            // copyFrom requires same length, so loop
            NativeArray<GEBodyState>.Copy(oldEphem, 0, gePhysicsJob.ephemerisData, 0, size);
            oldEphem.Dispose();
            GEBodyState eState = new GEBodyState();
            double rScale = geScaler.ScaleLenWorldToGE(1.0);
            double vScale = geScaler.ScaleVelocityWorldToGE(1.0);
            double tScale = geScaler.ScaleTimeWorldToGE(1.0);
            // add new ephem data, converting to ge scale as we go
            for (int i = 0; i < numPoints; i++) {
                eState.r = rScale * ephemData.data[i].r;
                eState.v = vScale * ephemData.data[i].v;
                eState.t = tScale * ephemData.data[i].t;
                gePhysicsJob.ephemerisData[size + i] = eState;
            }

            PropInfo propInfo = new PropInfo();
            propInfo.baseIndex = size;
            propInfo.numPoints = numPoints;
            propInfo.relative = ephemData.relative;
            propInfo.centerId = centerId;
            propInfo.tInterval = (gePhysicsJob.ephemerisData[size + numPoints - 1].t - gePhysicsJob.ephemerisData[size].t) / (numPoints - 1);
            propInfo.tStart = gePhysicsJob.ephemerisData[size].t;
            //Debug.LogFormat("tend={0} tstart={1} interval={2}", gePhysicsJob.ephemerisData[numPoints - 1].t,
            //    gePhysicsJob.ephemerisData[0].t, propInfo.tInterval);
            gePhysicsJob.ephemPropInfo[pIndex] = propInfo;

        }

        /// <summary>
		/// Free the memory in the global flat ephemData array.
		///
		/// This is a klunky, greedy implementation:
		/// - find the entry range
		/// - copy down everything higher
		/// 
		/// </summary>
		/// <param name="gePhysicsJob"></param>
		/// <param name="pIndex"></param>
        public static void EphemDataFree(ref GEPhysicsCore.GEPhysicsJob gePhysicsJob, int pIndex)
        {
            NativeArray<GEBodyState> oldEphem = gePhysicsJob.ephemerisData;
            int n = gePhysicsJob.ephemPropInfo[pIndex].numPoints;
            int newSize = gePhysicsJob.ephemerisData.Length - n;
            int baseIndex = gePhysicsJob.ephemPropInfo[pIndex].baseIndex;
            gePhysicsJob.ephemerisData = new NativeArray<GEBodyState>(newSize, Allocator.Persistent);
            UnityEngine.Debug.LogFormat("Freeing {0} points, new size {1} base={2}", n, newSize, baseIndex);
            if (newSize > 0) {
                // copy over up to the gap
                NativeArray<GEBodyState>.Copy(oldEphem, 0, gePhysicsJob.ephemerisData, 0, baseIndex);
                // copy from end of gap to the end
                if (baseIndex < newSize) {
                    NativeArray<GEBodyState>.Copy(oldEphem, baseIndex + n, gePhysicsJob.ephemerisData, baseIndex, newSize - baseIndex - n);
                }
            }
            oldEphem.Dispose();
            // now fix up the base index value for entries that were shuffled down
            for (int i = 0; i < gePhysicsJob.ephemPropInfo.Length; i++) {
                if (gePhysicsJob.ephemPropInfo[i].numPoints > 0 && gePhysicsJob.ephemPropInfo[i].baseIndex > baseIndex) {
                    PropInfo propInfo = gePhysicsJob.ephemPropInfo[i];
                    propInfo.baseIndex -= n;
                    gePhysicsJob.ephemPropInfo[i] = propInfo;
                }
            }
        }

    }
}
