using System;
using System.Collections.Generic;
using System.Linq;
using Vts.Common;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Implements ITissue.  Defines tissue geometries comprised of tetrahedra
    /// within cube volume with air around? 
    /// Data structures and processing reference: TIMOS software. 
    /// </summary>
    public class MultiTetrahedronInCubeTissue : TissueBase
    {
        private TetrahedronRegion _currentTetrahedronRegion;

        /// <summary>
        /// Creates an instance of a MultiTetrahedronInCubeTissue
        /// </summary>
        /// <param name="regions">list of tissue regions comprising tissue</param>
        /// <param name="meshDataFilename">filename of file containing mesh data output</param>
        /// <param name="absorptionWeightingType">absorption weighting type</param>
        /// <param name="phaseFunctionType">phase function type</param>
        /// <param name="russianRouletteWeightThreshold">photon weight threshold to turn on Russian Roulette</param>
        public MultiTetrahedronInCubeTissue(
            IList<ITissueRegion> regions, 
            string meshDataFilename,
            AbsorptionWeightingType absorptionWeightingType, 
            PhaseFunctionType phaseFunctionType,
            double russianRouletteWeightThreshold)
            : base(regions, absorptionWeightingType, phaseFunctionType,russianRouletteWeightThreshold)
        {
            MeshData = new TetrahedralMeshData(regions, meshDataFilename);
        }

        // Question: how to instantiate based on Input class if Regions not specified
        /// <summary>
        /// Creates an instance of a MultiTetrahedronInCubeTissue based on an input data class 
        /// </summary>
        /// <param name="input">multi-tetrahedron tissue input</param>
        /// <param name="absorptionWeightingType">absorption weighting type</param>
        /// <param name="phaseFunctionType">phase function type</param>
        /// <param name="russianRouletteWeightThreshold">russian roulette weight threshold</param>
        /// <remarks>air above and below tissue needs to be specified for a slab geometry</remarks>
        public MultiTetrahedronInCubeTissue(
            MultiTetrahedronInCubeTissueInput input, 
            AbsorptionWeightingType absorptionWeightingType, 
            PhaseFunctionType phaseFunctionType,
            double russianRouletteWeightThreshold)
            : this(input.Regions, input.MeshDataFilename, absorptionWeightingType, phaseFunctionType, russianRouletteWeightThreshold)
        {
        }
        /// <summary>
        /// Creates a default instance of a MultiTetrahedronInCubeTissue based on a homogeneous medium slab geometry
        /// and discrete absorption weighting
        /// </summary>
        public MultiTetrahedronInCubeTissue() 
            : this(new MultiTetrahedronInCubeTissueInput(), AbsorptionWeightingType.Discrete, PhaseFunctionType.HenyeyGreenstein, 0.0)
        {
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
        public override int GetRegionIndex(Position position)
        {
            return CurrentTetrahedronIndex;
        }
        
        /// <summary>
        /// Finds the distance to the next boundary independent of hitting it
        /// </summary>
        /// <param name="photon">current photon state</param>
        public override double GetDistanceToBoundary(Photon photon)
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
        public override bool OnDomainBoundary(Position position)
        {
            // check 
            return true;
        }
        /// <summary>
        /// method to determine index of region photon is about to enter
        /// </summary>
        /// <param name="photon">photon info including position and direction</param>
        /// <returns>region index</returns>
        public override int GetNeighborRegionIndex(Photon photon)
        {
            
            return 1;
        }
        /// <summary>
        /// method to determine photon state type of photon exiting tissue boundary
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override PhotonStateType GetPhotonDataPointStateOnExit(Position position)
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
        public override Direction GetReflectedDirection(
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
        public override Direction GetRefractedDirection(
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
        public override double GetAngleRelativeToBoundaryNormal(Photon photon)
        {
            // need to update this to handle angled plane of tetra side
            return Math.Abs(photon.DP.Direction.Uz); // abs will work for upward normal and downward normal
        }
    }
}
