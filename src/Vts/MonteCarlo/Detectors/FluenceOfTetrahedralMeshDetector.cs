using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Vts.MonteCarlo.PhotonData;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo.Detectors
{
    /// <summary>
    /// Implements IHistoryDetector&lt;double[]&gt;.  Tally for Fluence(tetrahedra)
    /// where tetrahedra as defined by MultiTetrahedronInCubeTissue
    /// Note: this tally currently only works with discrete absorption weighting and analog
    /// </summary>
    [KnownType(typeof(FluenceOfTetrahedralMeshDetector))]
    public class FluenceOfTetrahedralMeshDetector : IHistoryDetector<double[]>
    {
        private Func<PhotonDataPoint, PhotonDataPoint, int, double> _absorptionWeightingMethod;

        private ITissue _tissue;
        private bool _tallySecondMoment;
        private IList<OpticalProperties> _ops;
        /// <summary>
        /// constructor for fluence as a function of tetrahedra detector input
        /// </summary>
        /// <param name="tetrahedralMeshData">data describing tetrahedral mesh</param>
        /// <param name="tissue">tissue definition</param>
        /// <param name="tallySecondMoment">flag indicating whether to tally second moment info for error results</param>
        /// <param name="name">detector name</param>
        public FluenceOfTetrahedralMeshDetector(
            ITissue tissue,
            bool tallySecondMoment,
            String name
            )
        {
            TetrahedralMesh = ((MultiTetrahedronInCubeTissue) tissue).MeshData;
            Mean = new double[TetrahedralMesh.TetrahedronRegions.Length - 1];
            _tallySecondMoment = tallySecondMoment;
            SecondMoment = null;
            if (_tallySecondMoment)
            {
                SecondMoment = new double[TetrahedralMesh.TetrahedronRegions.Length - 1];
            }
            TallyType = TallyType.FluenceOfXAndYAndZ;
            Name = name;
            _absorptionWeightingMethod = AbsorptionWeightingMethods.GetVolumeAbsorptionWeightingMethod(tissue, this);

            TallyCount = 0;
            _tissue = tissue;
            _ops = tissue.Regions.Select(r => r.RegionOP).ToArray();
        }

        /// <summary>
        /// Returns an instance of FluenceOfXAndYAndZDetector (for serialization purposes only)
        /// </summary>
        public FluenceOfTetrahedralMeshDetector()
            : this(
            new MultiTetrahedronInCubeTissue(), 
            true,
            TallyType.FluenceOfTetrahedralMesh.ToString())
        {
        }
        /// <summary>
        /// detector mean
        /// </summary>
        [IgnoreDataMember]
        public double[] Mean { get; set; }
        /// <summary>
        /// detector second moment
        /// </summary>
        [IgnoreDataMember]
        public double[] SecondMoment { get; set; }

        /// <summary>
        /// detector identifier
        /// </summary>
        public TallyType TallyType { get; set; }
        /// <summary>
        /// detector name, default uses TallyType, but can be user specified
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// number of zs detector gets tallied to
        /// </summary>
        public long TallyCount { get; set; }
        /// <summary>
        /// tetrahedron mesh data
        /// </summary>
        public TetrahedralMeshData TetrahedralMesh { get; set; }

        /// <summary>
        /// method to tally given two consecutive photon data points
        /// </summary>
        /// <param name="previousDP">previous data point</param>
        /// <param name="dp">current data point</param>
        /// <param name="currentRegionIndex">index of region photon current is in</param>
        public void TallySingle(PhotonDataPoint previousDP, PhotonDataPoint dp, int currentRegionIndex)
        {
            // determine which tetrahedron dp is in, index = it
            var it = 1;

            var weight = _absorptionWeightingMethod(previousDP, dp, currentRegionIndex);

            var regionIndex = currentRegionIndex;

            if (weight != 0.0)
            {
                Mean[it] += weight / _ops[regionIndex].Mua;
                if (_tallySecondMoment)
                {
                    SecondMoment[it] += (weight / _ops[regionIndex].Mua) * (weight / _ops[regionIndex].Mua);
                }
                TallyCount++;
            }
        }
        /// <summary>
        /// method to tally to detector
        /// </summary>
        /// <param name="photon">photon data needed to tally</param>
        public void Tally(Photon photon)
        {
            PhotonDataPoint previousDP = photon.History.HistoryData.First();
            foreach (PhotonDataPoint dp in photon.History.HistoryData.Skip(1))
            {
                TallySingle(previousDP, dp, _tissue.GetRegionIndex(dp.Position)); // unoptimized version, but HistoryDataController calls this once
                previousDP = dp;
            }
        }

        private double AbsorbAnalog(double mua, double mus, double previousWeight, double weight, PhotonStateType photonStateType)
        {
            if (photonStateType.HasFlag(PhotonStateType.Absorbed))
            {
                weight = previousWeight; 
            }
            else
            {
                weight = 0.0;
            }
            return weight;
        }

        private double AbsorbDiscrete(double mua, double mus, double previousWeight, double weight, PhotonStateType photonStateType)
        {
            if (previousWeight == weight) // pseudo collision, so no tally
            {
                weight = 0.0;
            }
            else
            {
                weight = previousWeight * mua / (mua + mus);
            }
            return weight;
        }

        private double AbsorbContinuous(double mua, double mus, double previousWeight, double weight, PhotonStateType photonStateType)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// method to normalize detector results after numPhotons launched
        /// </summary>
        /// <param name="numPhotons">number of photons launched</param>
        public void Normalize(long numPhotons)
        {
            for (int it = 0; it < TetrahedralMesh.TetrahedronRegions.Length - 1; it++)
            {
                // determine volume of tetrahedron
                var normalizationFactor = 1;
                Mean[it] /= normalizationFactor * numPhotons;
                if (_tallySecondMoment)
                {
                    SecondMoment[it] /= normalizationFactor * normalizationFactor * numPhotons;
                }
  
            }

        }
        /// <summary>
        /// method to determine if photon within detector, i.e. in NA, etc.
        /// </summary>
        /// <param name="dp">photon data point</param>
        /// <returns>method always returns true</returns>
        public bool ContainsPoint(PhotonDataPoint dp)
        {
            return true;
        }

    }
}