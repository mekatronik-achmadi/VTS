using System;
using System.Collections.Generic;
using Vts.Common;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Defines Triangle given 3 nodes.  Facilitates TetrahedralRegion class.
    /// In future can possibly merge with FemModeling/MGRTE/2D/DataStructures/MeshInputs/SpatialMesh 
    /// </summary>
    public class TriangleRegion 
    {
        /// <summary>
        /// class specifies Triangle tissue region.
        /// </summary>
        /// <param name="nodes">array of nodes</param>
        public TriangleRegion(Position[] nodes)
        {
            Nodes = nodes;
            NodeIndices = new int[4];
            TetrahedronIndices = new int[2] { -1, -1 }; // -1 denotes not set
            InitializeTriangleRegion(nodes);
        }

        /// <summary>
        /// default constructor defines Triangle at origin 
        /// </summary>
        public TriangleRegion() : this(
            new Position[]
                {
                    new Position(0, 0, 0),
                    new Position(0, 0, 1),
                    new Position(0, 1, 0)
                }) {}

        /// <summary>
        /// nodes of Triangle
        /// </summary>
        public IList<Position> Nodes { get; private set; }
        /// <summary>
        /// int indicating boundary index of triangle 
        /// </summary>
        public int BoundaryIndex { get; set; }
        /// <summary>
        /// normal vector to this triangle
        /// </summary>
        public Direction Normal { get; set; }
        /// <summary>
        /// area of triangle
        /// </summary>
        public double Area { get; set; }
        /// <summary>
        ///  triangle lies in plane a n0 + b n1 + c n2 + d = 0 where
        /// (n0,n1,n2) is normal vector
        /// </summary>
        public double D { get; set; }
        /// <summary>
        /// indices of nodes that comprise triangle
        /// </summary>
        public int[] NodeIndices { get; set; }
        /// <summary>
        /// index of triangle
        /// </summary>
        public int TriangleIndex { get; set; }
        /// <summary>
        /// indices of the tetrahedra that this triangle supports
        /// </summary>
        public int[] TetrahedronIndices { get; set; }
        /// <summary>
        /// number of tetrahedrons that contain this triangle
        /// </summary>
        public int NumberOfContainingTetrahedrons { get; set; }

        /// <summary>
        /// method that initializes information about triangle. Algorithm taken from
        /// G. Wong TIMOS.cpp
        /// </summary>
        /// <param name="nodes">three positions that define triangle in 3 space</param>
        private void InitializeTriangleRegion(Position[] nodes)
        {
            // determine coordinates of each triangle
            var x0 = Nodes[0].X;
            var x1 = Nodes[1].X;
            var x2 = Nodes[2].X;
            var y0 = Nodes[0].Y;
            var y1 = Nodes[1].Y;
            var y2 = Nodes[2].Y;
            var z0 = Nodes[0].Z;
            var z1 = Nodes[1].Z;
            var z2 = Nodes[2].Z;
            // determine normal
            var ax = x1 - x0;
            var ay = y1 - y0;
            var az = z1 - z0;
            var bx = x2 - x0;
            var by = y2 - y0;
            var bz = z2 - z0;
            var nx = ay * bz - az * by;
            var ny = az * bx - ax * bz;
            var nz = ax * by - ay * bx;
            var norm = Math.Sqrt(nx * nx + ny * ny + nz * nz);
            Normal = new Direction(nx / norm, ny / norm, nz / norm); // unit normal
            Area = norm / 2.0;
            D = -(x0*Normal.Ux + y0*Normal.Uy + z0*Normal.Uz);
        }
        public static int CompareTrianglesByNodeIndices(TriangleRegion a, TriangleRegion b)
        {
            if ((a.NodeIndices[0] < b.NodeIndices[0]) ||
                   (a.NodeIndices[0] == b.NodeIndices[0] && a.NodeIndices[1] < b.NodeIndices[1]) ||
                   (a.NodeIndices[0] == b.NodeIndices[0] && a.NodeIndices[1] == b.NodeIndices[1] &&
                   a.NodeIndices[2] < b.NodeIndices[2]))
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}
