using System;
using System.Collections.Generic;
using System.Linq;
using Vts.Common;
using Vts.Extensions;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Implements ITissue.  Defines a tissue geometry comprised of an
    /// inclusion embedded within a layered slab.
    /// </summary>
    public class SingleInclusionTissue : MultiLayerTissue, ITissue
    {
        private ITissueRegion _inclusionRegion;
        private int _inclusionRegionIndex;
        private int _layerRegionIndexOfInclusion;

        /// <summary>
        /// Creates an instance of a SingleInclusionTissue
        /// </summary>
        /// <param name="inclusionRegion">The single inclusion (must be contained completely within a layer region)</param>
        /// <param name="layerRegions">The tissue layers</param>
        public SingleInclusionTissue(
            ITissueRegion inclusionRegion,
            IList<ITissueRegion> layerRegions)
            : base(layerRegions)
        {
            // overwrite the Regions property in the TissueBase class (will be called last in the most derived class)
            Regions = layerRegions.Concat(inclusionRegion).ToArray();

            _inclusionRegion = inclusionRegion;
            _inclusionRegionIndex = layerRegions.Count; // index is, by convention, after the layer region indices
            _layerRegionIndexOfInclusion = Enumerable.Range(0, layerRegions.Count)
                .FirstOrDefault(i => ((LayerTissueRegion) layerRegions[i]).ContainsPosition(_inclusionRegion.Center));
        }

        /// <summary>
        /// Creates a default instance of a SingleInclusionTissue
        /// </summary>
        public SingleInclusionTissue()
            : this(
                new EllipsoidTissueRegion(),
                new MultiLayerTissueInput().Regions)
        {
        }

        /// <summary>
        /// method to get tissue region index of photon's current position
        /// </summary>
        /// <param name="position">photon Position</param>
        /// <returns>integer tissue region index</returns>
        public int GetRegionIndex(Position position)
        {
            // if it's in the inclusion, return "3", otherwise, call the layer method to determine
            return _inclusionRegion.ContainsPosition(position) ? _inclusionRegionIndex : base.GetRegionIndex(position);
        }

        // todo: DC - worried that this is "uncombined" with GetDistanceToBoundary() from an efficiency standpoint
        // note, however that there are two overloads currently for RayIntersectBoundary, one that does extra work to calc distances
        /// <summary>
        /// method to get index of neighbor tissue region when photon on boundary of two regions
        /// </summary>
        /// <param name="photon">Photon</param>
        /// <returns>index of neighbor index</returns>
        public int GetNeighborRegionIndex(Photon photon)
        {
            // first, check what region the photon is in
            int regionIndex = photon.CurrentRegionIndex;

            // if we're outside the layer containing the inclusion, then just call the base method
            //if ((regionIndex != _inclusionRegionIndex) && (regionIndex != _layerRegionIndexOfInclusion))
            //{
            //    return base.GetNeighborRegionIndex(photon);
            //}

            // if we're in the layer region of the inclusion, could be on boundary of layer
            if ((regionIndex == _layerRegionIndexOfInclusion) &&
                !Regions[_layerRegionIndexOfInclusion].OnBoundary(photon.DP.Position))
            {
                return _inclusionRegionIndex;
            }

            if (regionIndex == _inclusionRegionIndex)
            {
                return _layerRegionIndexOfInclusion;
            }

            //// if we're actually inside the inclusion
            //if (_inclusionRegion.ContainsPosition(photon.DP.Position))
            //{
            //    // then the neighbor region is the layer containing the current photon position
            //    return layerRegionIndex;
            //}

            //// otherwise, it depends on which direction the photon's pointing from within the layer

            //// if the ray intersects the inclusion, the neighbor is the inclusion
            //if( _inclusionRegion.RayIntersectBoundary(photon) )
            //{
            //    return _inclusionRegionIndex;
            //}

            // otherwise we can do this with the base class method
            return base.GetNeighborRegionIndex(photon);
        }

        /// <summary>
        /// method to get distance from current photon position and direction to boundary of region
        /// </summary>
        /// <param name="photon">Photon</param>
        /// <returns>distance to boundary</returns>
        public double GetDistanceToBoundary(Photon photon)
        {
            // first, check what region the photon is in
            int regionIndex = photon.CurrentRegionIndex;

            if ((regionIndex == _layerRegionIndexOfInclusion) || (regionIndex == _inclusionRegionIndex))
            {
                // check if current track will hit the inclusion boundary, returning the correct distance
                double distanceToBoundary;
                if (_inclusionRegion.RayIntersectBoundary(photon, out distanceToBoundary))
                {
                    return distanceToBoundary;
                }
                else // otherwise, check that a projected track will hit the inclusion boundary
                {
                    var projectedPhoton = new Photon();
                    projectedPhoton.DP = new PhotonDataPoint(photon.DP.Position, photon.DP.Direction, photon.DP.Weight,
                        photon.DP.TotalTime, photon.DP.StateFlag);
                    projectedPhoton.S = 100;
                    if (_inclusionRegion.RayIntersectBoundary(projectedPhoton, out distanceToBoundary))
                    {
                        return distanceToBoundary;
                    }
                }
            }

            // if not hitting the inclusion, call the base (layer) method
            return base.GetDistanceToBoundary(photon);
        }

        /// <summary>
        /// method that provides reflected direction when phton reflects off boundary
        /// reference: https://graphics.stanford.edu/courses/cs148-10-summer/docs/2006--degreve--reflection_refraction.pdf
        /// </summary>
        /// <param name="currentPosition">Position</param>
        /// <param name="currentDirection">Direction</param>
        /// <returns>new Direction</returns>
        public Direction GetReflectedDirection(
            Position currentPosition,
            Direction currentDirection)
        {
            // needs to call MultiLayerTissue when crossing top and bottom layer
            if (base.OnDomainBoundary(currentPosition))
            {
                return base.GetReflectedDirection(currentPosition, currentDirection);
            }
            else
            {
                if (_inclusionRegion.OnBoundary(currentPosition))
                {
                    Direction normal = _inclusionRegion.SurfaceNormal(currentPosition);
                    var photon = new Photon();
                    photon.DP.Position = currentPosition;
                    photon.DP.Direction = currentDirection;
                    var cosTheta = GetAngleRelativeToBoundaryNormal(photon);
                    return new Direction(
                        currentDirection.Ux + 2 * cosTheta * normal.Ux,
                        currentDirection.Uy + 2 * cosTheta * normal.Uy,
                        currentDirection.Uz + 2 * cosTheta * normal.Uz);
                }
                else
                {
                    return currentDirection;
                }
            }
        }

        /// <summary>
        /// method that provides refracted direction when photon refracts off boundary
        /// reference: https://graphics.stanford.edu/courses/cs148-10-summer/docs/2006--degreve--reflection_refraction.pdf
        /// </summary>
        /// <param name="currentPosition">Position</param>
        /// <param name="currentDirection">Direction</param>
        /// <returns>new Direction</returns>
        public Direction GetRefractedDirection(
            Position currentPosition,
            Direction currentDirection,
            double nCurrent,
            double nNext,
            double cosThetaSnell)
        {
            // needs to call MultiLayerTissue when crossing top and bottom layer
            if (base.OnDomainBoundary(currentPosition))
            {
                return base.GetRefractedDirection(currentPosition, currentDirection, nCurrent, nNext, cosThetaSnell);
            }
            else
            {
                if (_inclusionRegion.OnBoundary(currentPosition)) // on boundary of inclusion
                {
                    var n = nCurrent / nNext;
                    Direction normal = _inclusionRegion.SurfaceNormal(currentPosition);
                    var photon = new Photon();
                    photon.DP.Position = currentPosition;
                    photon.DP.Direction = currentDirection;
                    var cosTheta = GetAngleRelativeToBoundaryNormal(photon);
                    var sinThetaSquared = n * n * (1 - cosTheta * cosTheta);
                    return new Direction(
                        n * currentDirection.Ux - (n + Math.Sqrt(1 - sinThetaSquared)) * normal.Ux,
                        n * currentDirection.Uy - (n + Math.Sqrt(1 - sinThetaSquared)) * normal.Uy,
                        n * currentDirection.Uz - (n + Math.Sqrt(1 - sinThetaSquared)) * normal.Uz);
                }
                else
                {
                    return currentDirection;
                }
            }
        }

        /// <summary>
        /// method to get cosine of the angle between photons current direction and boundary normal
        /// </summary>
        /// <param name="photon"></param>
        /// <returns>normal dot photon.Direction</returns>
        public double GetAngleRelativeToBoundaryNormal(Photon photon)
        {
            // needs to call MultiLayerTissue when crossing top and bottom layer
            if (base.OnDomainBoundary(photon.DP.Position))
            {
                return base.GetAngleRelativeToBoundaryNormal(photon);
            }
            else // on inclusion
            {
                Direction normal = _inclusionRegion.SurfaceNormal(photon.DP.Position);
                return Direction.GetDotProduct(normal, photon.DP.Direction);
            }
        }
    }
}
