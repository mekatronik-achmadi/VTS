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
        private int _currentTetrahedronIndex;
        private TetrahedronRegion _currentTetrahedronRegion;
        private IList<TriangleRegion> _triangles; // this is triangleList 
        private IList<int> _boundaryTriangles;

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
            MeshData = TetrahedralMeshData.FromFile(meshDataFilename);
            InitializeTriangleData();  // this performs PreProcessor processing
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

        public TetrahedralMeshData MeshData { get; private set; }

        private void InitializeTriangleData()
        {
            // add triangles to triangle list using MeshData
            var nodeOrder = new int[4,3]
                                {
                                    {0, 1, 2}, {0, 1, 3}, {0, 2, 3}, {1, 2, 3}
                                };
            // go through tetrahedron regions and add triangles to triangle list
            for (int i = 0; i < MeshData.TetrahedronRegions.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int k = (i - 1)*4 + j;
                    _triangles[k].NodeIndices[0] = MeshData.TetrahedronRegions[i].Triangles[j].NodeIndices[nodeOrder[j, 0]];
                    _triangles[k].NodeIndices[1] = MeshData.TetrahedronRegions[i].Triangles[j].NodeIndices[nodeOrder[j, 1]];
                    _triangles[k].NodeIndices[2] = MeshData.TetrahedronRegions[i].Triangles[j].NodeIndices[nodeOrder[j, 2]];
                    _triangles[k].NumberOfContainingTetrahedrons = 1;
                    _triangles[k].TetrahedronIndices[0] = i;
                    _triangles[k].TetrahedronIndices[1] = -1; // -1 means null
                }
            }
            // not sure if following 3 lines correct
            _triangles[4 * MeshData.TetrahedronRegions.Length].TetrahedronIndices[0] = MeshData.Nodes.Length;
            _triangles[4 * MeshData.TetrahedronRegions.Length].TetrahedronIndices[1] = MeshData.Nodes.Length;
            _triangles[4 * MeshData.TetrahedronRegions.Length].TetrahedronIndices[2] = MeshData.Nodes.Length;
            // sort the triangle list
            _triangles.ToList().Sort((x,y) => TriangleRegion.CompareTrianglesByNodeIndices(x,y));
            // eliminate duplicate triangles
            int currentTriangle = 0;
            int hole = 0;
            int numberOfBoundaryTriangles = 0;
            do
            {
                // check if duplicate
                if (_triangles[currentTriangle].NodeIndices[0] == _triangles[currentTriangle+1].NodeIndices[0] &&
                    _triangles[currentTriangle].NodeIndices[1] == _triangles[currentTriangle+1].NodeIndices[1] &&
                    _triangles[currentTriangle].NodeIndices[2] == _triangles[currentTriangle+1].NodeIndices[2])
                {
                    _triangles[hole].NodeIndices[0] = _triangles[currentTriangle].NodeIndices[0];
                    _triangles[hole].NodeIndices[1] = _triangles[currentTriangle].NodeIndices[1];
                    _triangles[hole].NodeIndices[2] = _triangles[currentTriangle].NodeIndices[2];
                    _triangles[hole].NumberOfContainingTetrahedrons = 2; // duplicate means triangle belongs to 2 tetras
                    _triangles[hole].TetrahedronIndices[0] = _triangles[currentTriangle].TetrahedronIndices[0];
                    _triangles[hole].TetrahedronIndices[1] = _triangles[currentTriangle+1].TetrahedronIndices[0];
                    currentTriangle += 2;
                }
                else // no duplicate
                {
                    _triangles[hole].NodeIndices[0] = _triangles[currentTriangle].NodeIndices[0];
                    _triangles[hole].NodeIndices[1] = _triangles[currentTriangle].NodeIndices[1];
                    _triangles[hole].NodeIndices[2] = _triangles[currentTriangle].NodeIndices[2];
                    _triangles[hole].NumberOfContainingTetrahedrons = _triangles[currentTriangle].NumberOfContainingTetrahedrons;
                    _triangles[hole].TetrahedronIndices[0] = _triangles[currentTriangle].TetrahedronIndices[0]; 
                    _triangles[hole].TetrahedronIndices[1] = _triangles[currentTriangle].TetrahedronIndices[1];
                    currentTriangle += 1;
                    ++numberOfBoundaryTriangles;
                }
                ++hole;
            } while (currentTriangle<4*MeshData.TetrahedronRegions.Length);
            // find internal/boundary triangles and update containing tetra info
            int index = 1;
            var numberOfTriangles = hole;
            int tempNumberOfBoundaryTriangles = 0;
            for (int i = 0; i < numberOfTriangles; i++)
            {
                if (_triangles[i].NumberOfContainingTetrahedrons == 1) // boundary triangle
                {
                    _triangles[index].TetrahedronIndices[0] = _triangles[i].TetrahedronIndices[0];
                    _triangles[index].TetrahedronIndices[1] = -1;
                    _triangles[index].BoundaryIndex = ++tempNumberOfBoundaryTriangles;
                    _boundaryTriangles[_triangles[index].BoundaryIndex] = i + 1;
                }
                else // triangle supports 2 tetras
                {
                    _triangles[index].NumberOfContainingTetrahedrons = 2;
                    _triangles[index].TetrahedronIndices[0] = _triangles[i].TetrahedronIndices[0];
                    _triangles[index].TetrahedronIndices[1] = _triangles[i].TetrahedronIndices[1];
                }
                for (int j = 0; j < _triangles[index].NumberOfContainingTetrahedrons; j++)
                {
                    for (int k = 0; k <= 3; k++)
                    {
                        // check that triangle assigned to correct number of tetras
                        if ((_triangles[index].TetrahedronIndices[j] < 1) || 
                            (_triangles[index].TetrahedronIndices[j] > _triangles[index].NumberOfContainingTetrahedrons))
                        {
                            throw new Exception("tetrahedral mesh is not correct");
                        }
                        if (_triangles[_triangles[index].TetrahedronIndices[i]].NodeIndices[k]==-1)
                        {
                            MeshData.TetrahedronRegions[_triangles[index].TetrahedronIndices[j]].
                                Triangles[k].  // not sure this k index is correct
                                NodeIndices[k] = index;
                            break;  // need to recode without this break
                        }
                    } // end for with index k
                } // end for with index j
                ++index;
            } // end for with index i
            // don't need to update triangle data, e.g. normal, area, etc., since done on instantiation
            // don't need to update tetrahedron data since done on instantiation
        }

        /// <summary>
        /// method to determine region index of region photon is currently in
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override int GetRegionIndex(Position position)
        {
            return _currentTetrahedronIndex;
        }
        
        /// <summary>
        /// Finds the distance to the next boundary independent of hitting it
        /// </summary>
        /// <param name="photon">current photon state</param>
        public override double GetDistanceToBoundary(Photon photon)
        {
            // first, check what region the photon is in
            int regionIndex = photon.CurrentRegionIndex;
            _currentTetrahedronIndex = regionIndex;

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
