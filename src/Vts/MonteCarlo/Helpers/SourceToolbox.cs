﻿using System;
using Vts.Common;
using Vts.Extensions;

namespace Vts.MonteCarlo.Sources
{
    /// <summary>
    /// Utilities shared by Source implementations.
    /// </summary>
    public class SourceToolbox
    {
        /// <summary>
        /// AngleDistributed provides a Lambertian Direction selected from thetaRange and phiRange
        /// </summary>
        /// <param name="thetaRange"></param>
        /// <param name="phiRange"></param>
        /// <param name="rng"></param>
        /// <returns>Direction</returns>
        /// CKH TODO:  add in rotation to solidAngleAxis
        public static Direction GetRandomAngleDistributedDirection(
            Direction solidAngleAxis, DoubleRange thetaRange, DoubleRange phiRange, Random rng)
        {
            double theta = rng.NextDouble(Math.Acos(thetaRange.Start), Math.Acos(thetaRange.Stop));
            double phi = rng.NextDouble(phiRange.Stop, phiRange.Start);
            return (new Direction(
                Math.Cos(theta) * Math.Sin(phi),
                Math.Sin(theta) * Math.Sin(phi),
                Math.Cos(phi)));
            // previous algorithm (uses rejection sampling)
            //public Direction AngleDistributed(double numericalAperture, double refractiveIndex,
            //    Generator rng)
            //double theta = 2.0 * Math.PI * rng.NextDouble();
            //double cost = Math.Cos(theta);
            //double sint = Math.Sin(theta);
            //double cosp, sinp;
            //do
            //{
            //    cosp = rng.NextDouble(); // has min max overload
            //    sinp = Math.Sqrt(1.0 - cosp * cosp);
            //} while (sinp > (numericalAperture / refractiveIndex));
            //return new Direction(cost * sinp, sint * sinp, cosp);
        }

        public static Position GetRandomFlatCircularPosition(Position center, double radius, Random rng)
        {
            if (radius == 0.0)
            {
                return (center);
            }

            double RN1 = rng.NextDouble();
            double RN2 = rng.NextDouble();
            double cosRN2 = Math.Cos(2 * Math.PI * RN2);
            double sinRN2 = Math.Sin(2 * Math.PI * RN2);
            return (new Position(
                center.X + radius * Math.Sqrt(RN1) * cosRN2,
                center.Y + radius * Math.Sqrt(RN1) * sinRN2,
                0.0));
        }

        public static Position GetRandomGaussianCircularPosition(Position center, double radius, Random rng)
        {
            if (radius == 0.0)
            {
                return (center);
            }

            double RN1 = rng.NextDouble();
            double RN2 = rng.NextDouble();
            double cosRN2 = Math.Cos(2 * Math.PI * RN2);
            double sinRN2 = Math.Sin(2 * Math.PI * RN2);

            /* Gaussian beam starting coordinates as given on p24 in Ch4 of AJW book  */
            if (RN1 == 1.0) RN1 = rng.NextDouble();
            return (new Position(
                center.X + radius * Math.Sqrt(-Math.Log(1.0 - RN1) / 2.0) * cosRN2,
                center.Y + radius * Math.Sqrt(-Math.Log(1.0 - RN1) / 2.0) * sinRN2,
                center.Z));
        }

        // todo: extend to arbitrary orientation
        // todo: CKH verify
        /// <summary>
        /// Returns a random position on a flat rectangular surface
        /// </summary>
        /// <param name="center"></param>
        /// <param name="lengthX">The x-length of the rectangle</param>
        /// <param name="lengthY">The x-length of the rectangle</param>
        /// <param name="rng">The random number generator</param>
        /// <returns></returns>
        /// <remarks>Custom orientation is not yet enabled.</remarks> 
        public static Position GetRandomFlatRectangularPosition(Position center, double lengthX, double lengthY, Random rng)
        {
            var position = new Position { Z = center.Z };

            if (lengthX == 0.0) { position.X = lengthX; }
            else
            {
                position.X = center.X + lengthX * (rng.NextDouble() - 0.5);
            }

            if (lengthY == 0.0) { position.Y = lengthY; }
            else
            {
                position.Y = center.Y + lengthY * (rng.NextDouble() - 0.5);
            }

            return position;
        }

        /// <summary>
        /// Returns a random position on a line of a specified length
        /// </summary>
        /// <param name="center">The center of the line</param>
        /// <param name="lineOrientation">The unit vector line orientation. (Must be normalized!)</param>
        /// <param name="length">The length of the line</param>
        /// <param name="rng">The random number generator</param>
        /// <returns></returns>
        /// <remarks>Custom orientation is not yet enabled.</remarks>
        public static Position GetRandomLinePosition(Position center, Direction lineOrientation, double length, Random rng)
        {
            var position = center;

            // if the length is zero, this is a point source - just return the center location
            if (length == 0.0) { return position; }

            var positionAlongAxis = length * (rng.NextDouble() - 0.5);

            // need to calculate X,Y,Z location based on projection of 
            // positionAlongAxis along the direction of lineOrientation
            // todo: CKH help

            return position;
        }

        /// <summary>
        /// Returns a random radial direction [0,2*Pi] along a specified axis
        /// </summary>
        /// <param name="axisOrientation">The unit vector radial axis orientation. (Must be normalized!)</param>
        /// <param name="rng">The random number generator</param>
        /// <returns></returns>
        /// <remarks>Custom orientation is not yet enabled.</remarks>
        public static Direction GetRandomRadialDirection(Direction axisOrientation, Random rng)
        {
            var theta = 2 * Math.PI * rng.NextDouble();

            // need to calculate the direction based on projection of 
            // theta along the direction of axisOrientation
            // todo: CKH help
            var direction = new Direction();

            return direction;
        }
    }
}