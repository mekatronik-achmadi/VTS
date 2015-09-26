using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Vts.Common;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Implements ITissueInput.  Defines input to MultiTetrahedronInCubeTissue class.
    /// </summary>
    public class MultiTetrahedronInCubeTissueInput : ITissueInput
    {
        private ITissueRegion[] _regions;
        private string _meshDataFilename;

        /// <summary>
        /// constructor for Multi-tetrahedron in cube tissue input
        /// </summary>
        /// <param name="regions">list of tissue regions comprising tissue</param>
        // Question: ITissue expects an array of ITissueRegion.  Can I use filename 
        // to instantiate Regions?
        public MultiTetrahedronInCubeTissueInput(ITissueRegion[] regions, string meshDataFilename)
        {
            _regions = regions;
            _meshDataFilename = meshDataFilename;
        }

        /// <summary>
        /// MultiTetrahedronInCubeTissue default constructor provides homogeneous tissue
        /// </summary>
        public MultiTetrahedronInCubeTissueInput()
            : this(
                new ITissueRegion[]
                { 
                    // need to determine how to bring in list of tetrahedra
                    new TetrahedronTissueRegion()
                },
        "cube")
        {
        }
        /// <summary>
        /// tissue identifier
        /// </summary>
        public string TissueType { get { return "MultiTetrahedronInCube"; } }
        /// <summary>
        /// list of tissue regions comprising tissue
        /// </summary>
        public ITissueRegion[] Regions { get { return _regions; } set { _regions = value; } }
        /// <summary>
        /// filename of input mesh data
        /// </summary>
        public string MeshDataFilename { get { return _meshDataFilename; } set { _meshDataFilename = value; } }

        /// <summary>
        ///// Required factory method to create the corresponding 
        ///// ITissue based on the ITissueInput data
        /// </summary>
        /// <param name="awt">Absorption Weighting Type</param>
        /// <param name="pft">Phase Function Type</param>
        /// <param name="russianRouletteWeightThreshold">Russian Roulette Weight Threshold</param>
        /// <returns></returns>
        public ITissue CreateTissue(AbsorptionWeightingType awt, PhaseFunctionType pft, double russianRouletteWeightThreshold)
        {
            var t = new MultiTetrahedronInCubeTissue(Regions, MeshDataFilename);

            t.Initialize(awt, pft, russianRouletteWeightThreshold);

            return t;
        }
    }
    /// <summary>
    /// Implements ITissue.  Defines tissue geometries comprised of tetrahedra
    /// within cube volume with air around? 
    /// Data structures and processing reference: TIMOS software. 
    /// </summary>
    public class MultiTetrahedronInCubeTissue : TissueBase, ITissue
    {
        private TetrahedronTissueRegion _currentTetrahedronRegion;

        /// <summary>
        /// Creates an instance of a MultiTetrahedronInCubeTissue
        /// </summary>
        /// <param name="regions">list of tissue regions comprising tissue</param>
        /// <param name="meshDataFilename">filename of file containing mesh data output</param>
            public MultiTetrahedronInCubeTissue(
            IList<ITissueRegion> regions, 
            string meshDataFilename)
            : base(regions)
        {
            MeshData = new TetrahedralMeshData(regions, meshDataFilename);
        }

        /// <summary>
        /// MeshData contains all mesh related data
        /// </summary>
        public TetrahedralMeshData MeshData { get; private set; }
        /// <summary>
        /// index of current tetrahedron where photon is
        /// </summary>
        public int CurrentTetrahedronIndex { get; private set; }

        /// <summary>
        /// method to determine region index of region photon is currently in
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int GetRegionIndex(Position position)
        {
            return CurrentTetrahedronIndex;
        }
        
        /// <summary>
        /// Finds the distance to the next boundary independent of hitting it
        /// </summary>
        /// <param name="photon">current photon state</param>
        public double GetDistanceToBoundary(Photon photon)
        {
            // first, check what region the photon is in
            int regionIndex = photon.CurrentRegionIndex;
            CurrentTetrahedronIndex = regionIndex;

            // check if current track will hit the tetrahedron boundary, returning the correct distance
            double distanceToBoundary = double.PositiveInfinity;
            if (_currentTetrahedronRegion.RayIntersectBoundary(photon, out distanceToBoundary))
            {
                return distanceToBoundary;
            }
            else // otherwise, check that a projected track will hit the inclusion boundary
            {
                var projectedPhoton = new Photon();
                projectedPhoton.DP = new PhotonDataPoint(photon.DP.Position, photon.DP.Direction, photon.DP.Weight,
                    photon.DP.TotalTime, photon.DP.StateFlag);
                projectedPhoton.S = 100;
                if (_currentTetrahedronRegion.RayIntersectBoundary(projectedPhoton, out distanceToBoundary))
                {
                    return distanceToBoundary;
                }
            }
            return distanceToBoundary;
        }
        /// <summary>
        /// method to determine if on boundary of cube
        /// </summary>
        /// <param name="position">photon position</param>
        /// <returns></returns>
        public bool OnDomainBoundary(Position position)
        {
            // check 
            return true;
        }
        /// <summary>
        /// method to determine index of region photon is about to enter
        /// </summary>
        /// <param name="photon">photon info including position and direction</param>
        /// <returns>region index</returns>
        public int GetNeighborRegionIndex(Photon photon)
        {
            
            return 1;
        }
        /// <summary>
        /// method to determine photon state type of photon exiting tissue boundary
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public PhotonStateType GetPhotonDataPointStateOnExit(Position position)
        {
            // need to update this to account for exiting all sides of cube
            if (position.Z < 1e-10)
            {
                return PhotonStateType.PseudoReflectedTissueBoundary;
            }
            
            return PhotonStateType.PseudoTransmittedTissueBoundary;
        }
        /// <summary>
        /// method to determine direction of reflected photon
        /// </summary>
        /// <param name="positionCurrent"></param>
        /// <param name="directionCurrent"></param>
        /// <returns></returns>
        public Direction GetReflectedDirection(
            Position positionCurrent, 
            Direction directionCurrent)
        {
            return new Direction(
                directionCurrent.Ux,
                directionCurrent.Uy,
                -directionCurrent.Uz);
        }
        /// <summary>
        /// method to determine refracted direction of photon
        /// </summary>
        /// <param name="positionCurrent">current photon position</param>
        /// <param name="directionCurrent">current photon direction</param>
        /// <param name="nCurrent">refractive index of current region</param>
        /// <param name="nNext">refractive index of next region</param>
        /// <param name="cosThetaSnell">cos(theta) resulting from Snell's law</param>
        /// <returns>direction</returns>
        public Direction GetRefractedDirection(
            Position positionCurrent, 
            Direction directionCurrent, 
            double nCurrent, 
            double nNext, 
            double cosThetaSnell)
        {
            // need to update this to refract off plane at angle that is side of tetra
            if (directionCurrent.Uz > 0)
                return new Direction(
                    directionCurrent.Ux * nCurrent / nNext,
                    directionCurrent.Uy * nCurrent / nNext,
                    cosThetaSnell);
            else
                return new Direction(
                    directionCurrent.Ux * nCurrent / nNext,
                    directionCurrent.Uy * nCurrent / nNext,
                    -cosThetaSnell);
        }
        /// <summary>
        /// method to get angle between photons current direction and boundary normal
        /// </summary>
        /// <param name="photon"></param>
        /// <returns></returns>
        public double GetAngleRelativeToBoundaryNormal(Photon photon)
        {
            // need to update this to handle angled plane of tetra side
            return Math.Abs(photon.DP.Direction.Uz); // abs will work for upward normal and downward normal
        }
    }
}
