using System;
using System.Collections.Generic;
using Vts.Common;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Implements ITissueRegion.  Defines tetrahedron given 4 nodes.
    /// </summary>
    public class TetrahedronRegion : ITissueRegion
    {
        private bool _onBoundary = false;
        /// <summary>
        /// class specifies tetrahedron tissue region.
        /// </summary>
        /// <param name="nodes">list of nodes</param>
        /// <param name="op">OpticalProperties of tetrahedron</param>
        public TetrahedronRegion(IList<Position> nodes, OpticalProperties op)
        {
            RegionOP = op;
        }
        /// <summary>
        /// default constructor defines tetrahedron at origin 
        /// </summary>
        public TetrahedronRegion() : this (
            new List<Position>() { 
                new Position(0, 0, 0),
                new Position(0, 0, 1),
                new Position(0, 1, 0),
                new Position(1, 0, 0) },
            new OpticalProperties(0.05, 1.0, 0.8, 1.4)) {}

        /// <summary>
        /// optical properties of tetrahedron
        /// </summary>
        public OpticalProperties RegionOP { get; set; }
        /// <summary>
        /// center of tetrahedron (not sure this makes sense for this shape)
        /// </summary>
        public Position Center { get; set; }
        /// <summary>
        /// method to determine if given Position lies within tetrahedron
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>boolean, true if within, false otherwise</returns>
        public bool ContainsPosition(Position position)
        {
            // use http://steve.hollasch.net/cgindex/geometry/ptintet.html?
            double inside = 1;

                //if (inside < 0.9999999)
                if (inside < 0.9999999999)
                {
                    return true;
                }
                //else if (inside > 1.0000001)
                else if (inside > 1.00000000001)
                {
                    return false;
                }
                else  // on boundary
                {
                    _onBoundary = true;
                    //return false; // ckh try 8/21/11
                    return true;
                }
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
            double root1, root2, xto, yto, zto;
            double root = 0;
            return true;
        }        
    }
}
