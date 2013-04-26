using System;
using Vts.Common;
using System.IO;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Describes class for storing tetrahedron mesh info.
    /// </summary>
    public class TetrahedralMeshData 
    {
        /// <summary>
        /// Creates an instance of a TetrahedronMeshData
        /// </summary>
        public TetrahedralMeshData()
        {
            Nodes = null;
            TetrahedronRegions = null;
        }
        /// <summary>
        /// array of nodes of tetrahedrons
        /// not sure if I can get away with Ilist here and for regions
        /// </summary>
        public Position[] Nodes;
        /// <summary>
        /// array tissue optical properties
        /// </summary>
        public OpticalProperties[] OptProperties;
        /// <summary>
        /// array of tetrahedron regions and their optical properties
        /// </summary>
        public TetrahedronRegion[] TetrahedronRegions; 

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
            var data = new TetrahedralMeshData();
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
                    data.OptProperties[i] = new OpticalProperties(
                                double.Parse(bits[0]),
                                double.Parse(bits[1]),
                                double.Parse(bits[2]),
                                double.Parse(bits[3])
                            );
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
                    data.TetrahedronRegions[i] = new TetrahedronRegion(
                        new Position[]
                            {
                                data.Nodes[int.Parse(bits[0])],
                                data.Nodes[int.Parse(bits[1])],
                                data.Nodes[int.Parse(bits[2])],
                                data.Nodes[int.Parse(bits[3])]
                            },
                        new OpticalProperties(data.OptProperties[int.Parse(bits[4])]));
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
