using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Vts.Common;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Describes class for storing tetrahedron mesh info.
    /// </summary>
    public class TetrahedralMeshData 
    {
        private IList<TriangleRegion> _triangles; // this is triangleList 
        private IList<int> _boundaryTriangles;

        /// <summary>
        /// Creates an instance of a TetrahedronMeshData
        /// </summary>
        public TetrahedralMeshData(IList<ITissueRegion> regions, string meshDataFilename)
        {
            if (regions.Count == 0) // load mesh data from file if regions list is empty
            {
                FromFile(meshDataFilename);
            }
            else // load mesh data from parameter regions
            {
                TetrahedronRegions = regions.Select(region => (TetrahedronRegion)region).ToList();
                // the following distinct does not eliminate duplicates, but can work with duplicated list for now
                OptPropertiesList = (regions.Select(region => region.RegionOP)).Distinct().ToList();
                Nodes = TetrahedronRegions.SelectMany(t => t.Triangles).SelectMany(s => s.Nodes).ToList();
            }
            InitializeTriangleData();  // this performs PreProcessor processing
        }
        /// <summary>
        /// nodes of tetrahedrons
        /// </summary>
        public IList<Position> Nodes;
        /// <summary>
        /// list of distinct tissue optical properties
        /// </summary>
        public IList<OpticalProperties> OptPropertiesList;
        /// <summary>
        /// array of tetrahedron regions and their optical properties
        /// </summary>
        public IList<TetrahedronRegion> TetrahedronRegions;

        private void InitializeTriangleData()
        {
            // this is straight port from PreProcessor in TIMOS, could be optimized
            _triangles = new TriangleRegion[TetrahedronRegions.Count * 4];
            // add triangles to triangle list using MeshData
            var nodeOrder = new int[4, 3]
                                {
                                    {0, 1, 2}, {0, 1, 3}, {0, 2, 3}, {1, 2, 3}
                                };
            // go through tetrahedron regions and add triangles to triangle list
            for (int i = 0; i < TetrahedronRegions.Count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int k = i * 4 + j;
                    _triangles[k] = new TriangleRegion();
                    _triangles[k].NodeIndices[0] = TetrahedronRegions[i].Triangles[j].NodeIndices[nodeOrder[j, 0]];
                    _triangles[k].NodeIndices[1] = TetrahedronRegions[i].Triangles[j].NodeIndices[nodeOrder[j, 1]];
                    _triangles[k].NodeIndices[2] = TetrahedronRegions[i].Triangles[j].NodeIndices[nodeOrder[j, 2]];
                    _triangles[k].NumberOfContainingTetrahedrons = 1;
                    _triangles[k].TetrahedronIndices[0] = i;
                    _triangles[k].TetrahedronIndices[1] = -1; // -1 means null
                }
            }
            // not sure if following 3 lines correct
            _triangles[4 * (TetrahedronRegions.Count - 1)].TetrahedronIndices[0] = Nodes.Count;
            _triangles[4 * (TetrahedronRegions.Count - 1)].TetrahedronIndices[1] = Nodes.Count;
            _triangles[4 * (TetrahedronRegions.Count - 1)].TetrahedronIndices[2] = Nodes.Count;
            // sort the triangle list
            _triangles.ToList().Sort((x, y) => TriangleRegion.CompareTrianglesByNodeIndices(x, y));
            // eliminate duplicate triangles
            int currentTriangle = 0;
            int hole = 0;
            int numberOfBoundaryTriangles = 0;
            do
            {
                // check if duplicate
                if (_triangles[currentTriangle].NodeIndices[0] == _triangles[currentTriangle + 1].NodeIndices[0] &&
                    _triangles[currentTriangle].NodeIndices[1] == _triangles[currentTriangle + 1].NodeIndices[1] &&
                    _triangles[currentTriangle].NodeIndices[2] == _triangles[currentTriangle + 1].NodeIndices[2])
                {
                    _triangles[hole].NodeIndices[0] = _triangles[currentTriangle].NodeIndices[0];
                    _triangles[hole].NodeIndices[1] = _triangles[currentTriangle].NodeIndices[1];
                    _triangles[hole].NodeIndices[2] = _triangles[currentTriangle].NodeIndices[2];
                    _triangles[hole].NumberOfContainingTetrahedrons = 2; // duplicate means triangle belongs to 2 tetras
                    _triangles[hole].TetrahedronIndices[0] = _triangles[currentTriangle].TetrahedronIndices[0];
                    _triangles[hole].TetrahedronIndices[1] = _triangles[currentTriangle + 1].TetrahedronIndices[0];
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
            } while (currentTriangle < 4 * TetrahedronRegions.Count);
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
                        if (_triangles[_triangles[index].TetrahedronIndices[i]].NodeIndices[k] == -1)
                        {
                            TetrahedronRegions[_triangles[index].TetrahedronIndices[j]].
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
        /// Static helper method to simplify reading from files that have been output from TetGen.
        /// filename.nodes list the x,y,z coordinates of the nodes.
        /// filename.ele lists the nodes the comprise each tetrahedron in mesh
        /// filename.opt lists optical properties for different tissue types
        /// </summary>
        /// <param name="fileName">The base filename for the tetrahedral mesh (no ".xml")</param>
        /// <returns>A new instance of TetrahedronMeshData</returns>
        public static TetrahedralMeshData FromFile(string fileName)
        {
            var data = new TetrahedralMeshData(null, fileName);
            try
            {
                var srNodes = new StreamReader(fileName + ".node");
                // read number of nodes
                string text = srNodes.ReadLine();
                int numNodes = int.Parse(text);
                for (int i = 0; i < numNodes; i++)
                {
                    text = srNodes.ReadLine();
                    string[] bits = text.Split(' ');
                    data.Nodes[i] = new Position(double.Parse(bits[0]), double.Parse(bits[1]), double.Parse(bits[2]));
                } 
                var srOps = new StreamReader(fileName + ".opt");
                // read number of optical properties might have additional header line
                text = srOps.ReadLine();
                int numOps = int.Parse(text);
                for (int i = 0; i < numOps; i++)
                {
                    text = srOps.ReadLine();
                    string[] bits = text.Split(' ');
                    data.OptPropertiesList.Add(new OpticalProperties(
                                                   double.Parse(bits[0]),
                                                   double.Parse(bits[1]),
                                                   double.Parse(bits[2]),
                                                   double.Parse(bits[3])
                                                   ));
                }
                var srElements = new StreamReader(fileName + ".ele");
                // read number of elements, the indexes here refer to the indices of the Nodes that
                // comprise a tetrahedron element
                text = srElements.ReadLine();
                int numElements = int.Parse(text);
                for (int i = 0; i < numElements; i++)
                {
                    text = srElements.ReadLine();
                    string[] bits = text.Split(' ');
                    data.TetrahedronRegions.Add(new TetrahedronRegion(
                        new Position[]
                            {
                                data.Nodes[int.Parse(bits[0])],
                                data.Nodes[int.Parse(bits[1])],
                                data.Nodes[int.Parse(bits[2])],
                                data.Nodes[int.Parse(bits[3])]
                            },
                        new OpticalProperties(data.OptPropertiesList[int.Parse(bits[4])])));
                }  
            }
            catch (IOException e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

            return data; 
        }
    }
}
