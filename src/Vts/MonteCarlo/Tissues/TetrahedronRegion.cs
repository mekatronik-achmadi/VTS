using System;
using System.Collections.Generic;
using Vts.Common;
using Vts.SpectralMapping;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Implements ITissueRegion.  Defines tetrahedron given 4 nodes.
    /// </summary>
    public class TetrahedronRegion : ITissueRegion
    {
        private int _numTriangles = 4;  // number of triangles defined by tetrahedron
        private bool _onBoundary = false;
 
        /// <summary>
        /// class specifies tetrahedron tissue region.
        /// </summary>
        /// <param name="nodes">array of nodes</param>
        /// <param name="ops">Optical Property Index of tetrahedron from list in TetrahedralMeshData</param>
        public TetrahedronRegion(Position[] nodes, OpticalProperties ops)
        {
            RegionOP = ops;
            Center = new Position(
                (nodes[0].X + nodes[1].X + nodes[2].X + nodes[3].X) / 4,
                (nodes[0].Y + nodes[1].Y + nodes[2].Y + nodes[3].Y) / 4,
                (nodes[0].Z + nodes[1].Z + nodes[2].Z + nodes[3].Z) / 4);
            TissueRegionType = TissueRegionType.Tetrahedron;
            Triangles = new TriangleRegion[4];
            // tech question: does this order matter?
            Triangles[0] = new TriangleRegion(new Position[] { nodes[0], nodes[1], nodes[2] });
            Triangles[1] = new TriangleRegion(new Position[] { nodes[0], nodes[1], nodes[3] });
            Triangles[2] = new TriangleRegion(new Position[] { nodes[0], nodes[2], nodes[3] });
            Triangles[3] = new TriangleRegion(new Position[] { nodes[1], nodes[2], nodes[1] });
        }
        /// <summary>
        /// default constructor defines tetrahedron at origin 
        /// </summary>
        public TetrahedronRegion() : this (
            new Position[] { 
                new Position(0, 0, 0),
                new Position(0, 0, 1),
                new Position(0, 1, 0),
                new Position(1, 0, 0) },
            new OpticalProperties(0.01, 1, 0.8, 1.4)) {}

        /// <summary>
        /// optical properties of tetrahedron
        /// </summary>
        public OpticalProperties RegionOP { get; set; }
        /// <summary>
        /// center of tetrahedron
        /// </summary>
        public Position Center { get; private set; }
        /// <summary>
        /// tissue region identifier
        /// </summary>
        public TissueRegionType TissueRegionType { get; set; }
        /// <summary>
        /// volume of tetrahedron
        /// </summary>
        public double Volume { get; private set; }
        /// <summary>
        /// array of triangles that describe the 4 triangles
        /// </summary>
        public TriangleRegion[] Triangles { get; private set; }


        /// <summary>
        /// this method is not used by MultiTetrahedronInCubeTissue so always returns false
        /// method to determine if given Position lies within tetrahedron
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>boolean, true if within, false otherwise</returns>
        public bool ContainsPosition(Position position)
        {
            return false;
        }
        /// <summary>
        /// method to determine if given Position lies on boundary of tetrahedron
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>true if on boundary, false otherwise</returns>
        public bool OnBoundary(Position position)
        {
            return !ContainsPosition(position) && _onBoundary;
        }
        /// <summary>
        /// method to determine if photon track or ray intersects boundary of tetrahedron
        /// </summary>
        /// <param name="photon">Photon</param>
        /// <param name="distanceToBoundary">return: distance to boundary</param>
        /// <returns>boolean true if intersection, false otherwise</returns>
        public bool RayIntersectBoundary(Photon photon, out double distanceToBoundary)
        {
            distanceToBoundary = double.PositiveInfinity;
            bool hit = false;
            // determine normal distance from photon to each triangular face of the tetrahedron
            double[] temp = new double[4];
            temp[0] = Direction.GetDotProduct(Triangles[0].Normal, photon.DP.Direction);
            temp[1] = Direction.GetDotProduct(Triangles[1].Normal, photon.DP.Direction);
            temp[2] = Direction.GetDotProduct(Triangles[2].Normal, photon.DP.Direction);
            temp[3] = Direction.GetDotProduct(Triangles[3].Normal, photon.DP.Direction);
            for (int i = 0; i < _numTriangles; i++)
            {
                if (temp[i] < 0)
                {
                    var dum = Triangles[i].Normal.Ux * photon.DP.Position.X + 
                              Triangles[i].Normal.Uy * photon.DP.Position.Y +
                              Triangles[i].Normal.Uz * photon.DP.Position.Z + Triangles[i].D;
                    var distance = -dum/temp[i];
                    if (distance < distanceToBoundary)
                    {
                        distanceToBoundary = distance;
                    }
                }
            }
            if (distanceToBoundary < double.PositiveInfinity)
            {
                hit = true;
            }
            return hit;
        }        
    }
}
