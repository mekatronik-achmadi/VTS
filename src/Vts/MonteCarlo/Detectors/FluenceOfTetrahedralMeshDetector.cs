using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Vts.IO;
using Vts.MonteCarlo.PhotonData;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo.Detectors
{
    /// <summary>
    /// Tally for Fluence(tetrahedra)
    /// where tetrahedra as defined by MultiTetrahedronInCubeTissue
    /// Note: this tally currently only works with discrete absorption weighting and analog
    /// </summary>
    public class FluenceOfTetrahedralMeshDetectorInput : DetectorInput, IDetectorInput
    {
        /// <summary>
        /// constructor for fluence as a function of rho and Z detector input
        /// </summary>
        public FluenceOfTetrahedralMeshDetectorInput()
        {
            TallyType = "FluenceOfTetrahedralMesh";
            Name = "FluenceOfTetrahedralMesh";

            // modify base class TallyDetails to take advantage of built-in validation capabilities (error-checking)
            TallyDetails.IsVolumeTally = true;
            TallyDetails.IsCylindricalTally = false;
            TallyDetails.IsNotImplementedForCAW = true;
        }

        public IDetector CreateDetector()
        {
            return new FluenceOfRhoAndZDetector
            {
                // required properties (part of DetectorInput/Detector base classes)
                TallyType = this.TallyType,
                Name = this.Name,
                TallySecondMoment = this.TallySecondMoment,
                TallyDetails = this.TallyDetails,                
                // optional/custom detector-specific properties
            };
        }
    }
    /// <summary>
    /// Implements IDetector.  Tally for reflectance as a function  of Rho and Z.
    /// This implementation works for Analog, DAW and CAW processing.
    /// </summary>
    public class FluenceOfTetrahedralMeshDetector : Detector, IHistoryDetector
    {
        private Func<PhotonDataPoint, PhotonDataPoint, int, double> _absorptionWeightingMethod;
        private ITissue _tissue;
        private IList<OpticalProperties> _ops;

        /* ==== Place optional/user-defined input properties here. They will be saved in text (JSON) format ==== */
        /* ==== Note: make sure to copy over all optional/user-defined inputs from corresponding input class ==== */


        /* ==== Place user-defined output arrays here. They should be prepended with "[IgnoreDataMember]" attribute ==== */
        /* ==== Then, GetBinaryArrays() should be implemented to save them separately in binary format ==== */
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

        /* ==== Place optional/user-defined output properties here. They will be saved in text (JSON) format ==== */

        /// <summary>
        /// number of Zs detector gets tallied to
        /// </summary>
        public long TallyCount { get; set; }

        /// <summary>
        /// tetrahedron mesh data
        /// </summary>
        public TetrahedralMeshData TetrahedralMesh { get; set; }

        public void Initialize(ITissue tissue, Random rng)
        {
            // assign any user-defined outputs (except arrays...we'll make those on-demand)
            TallyCount = 0;

            // if the data arrays are null, create them (only create second moment if TallySecondMoment is true)
            Mean = Mean ?? new double[TetrahedralMesh.TetrahedronRegions.Count - 1];
            SecondMoment = SecondMoment ?? (TallySecondMoment ? new double[TetrahedralMesh.TetrahedronRegions.Count - 1] : null);

            // intialize any other necessary class fields here
            _absorptionWeightingMethod = AbsorptionWeightingMethods.GetVolumeAbsorptionWeightingMethod(tissue, this);
            _tissue = tissue;
            _ops = _tissue.Regions.Select(r => r.RegionOP).ToArray();
        }

        /// <summary>
        /// method to tally given two consecutive photon data points
        /// </summary>
        /// <param name="previousDP">previous data point</param>
        /// <param name="dp">current data point</param>
        /// <param name="currentRegionIndex">index of region photon current is in</param>
        public void TallySingle(PhotonDataPoint previousDP, PhotonDataPoint dp, int currentRegionIndex)
        {
            var it = currentRegionIndex;

            var weight = _absorptionWeightingMethod(previousDP, dp, currentRegionIndex);

            var regionIndex = currentRegionIndex;

            if (weight != 0.0)
            {
                Mean[it] += weight / _ops[regionIndex].Mua;
                if (TallySecondMoment)
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
            for (int it = 0; it < TetrahedralMesh.TetrahedronRegions.Count - 1; it++)
            {
                // determine volume of tetrahedron
                var normalizationFactor = 1;
                Mean[it] /= normalizationFactor * numPhotons;
                if (TallySecondMoment)
                {
                    SecondMoment[it] /= normalizationFactor * normalizationFactor * numPhotons;
                }  
            }
        }
        // this is to allow saving of large arrays separately as a binary file
        public BinaryArraySerializer[] GetBinarySerializers()
        {
            return new[] {
                new BinaryArraySerializer {
                    DataArray = Mean,
                    Name = "Mean",
                    FileTag = "",
                    WriteData = binaryWriter => {
                        for (int i = 0; i < TetrahedralMesh.TetrahedronRegions.Count - 1; i++) 
                        {
                            binaryWriter.Write(Mean[i]);
                        }
                    },
                    ReadData = binaryReader => {
                        Mean = Mean ?? new double[ TetrahedralMesh.TetrahedronRegions.Count - 1];
                        for (int i = 0; i <  TetrahedralMesh.TetrahedronRegions.Count - 1; i++) 
                        {
                            Mean[i] = binaryReader.ReadDouble(); 
                        }
                    }
                },
                // return a null serializer, if we're not serializing the second moment
                !TallySecondMoment ? null :  new BinaryArraySerializer {
                    DataArray = SecondMoment,
                    Name = "SecondMoment",
                    FileTag = "_2",
                    WriteData = binaryWriter => {
                        if (!TallySecondMoment || SecondMoment == null) return;
                        for (int i = 0; i < TetrahedralMesh.TetrahedronRegions.Count - 1; i++) 
                        {
                            binaryWriter.Write(SecondMoment[i]);
                        }     
                    },
                    ReadData = binaryReader => {
                        if (!TallySecondMoment || SecondMoment == null) return;
                        SecondMoment = new double[ TetrahedralMesh.TetrahedronRegions.Count - 1];
                        for (int i = 0; i < TetrahedralMesh.TetrahedronRegions.Count - 1; i++) 
                        {
                            SecondMoment[i] = binaryReader.ReadDouble();
                        }                       			            
                    },
                },
            };
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