﻿using System;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.Tissues
{
    /// <summary>
    /// Unit tests for EllipsoidTissueRegion
    /// </summary>
    [TestFixture]
    public class EllipsoidTissueRegionTests
    {
        private EllipsoidTissueRegion _ellipsoidTissueRegion;
        /// <summary>
        /// Validate general constructor of TissueRegion
        /// </summary>
        [OneTimeSetUp]
        public void create_instance_of_class()
        {
            _ellipsoidTissueRegion = new EllipsoidTissueRegion(
                new Position(0, 0, 3), 1.0, 1.0, 2.0, new OpticalProperties(0.01, 1.0, 0.8, 1.4));
        }
        /// <summary>
        /// Validate general constructor of TissueRegion
        /// </summary>
        [Test]
        public void validate_ellipsoid_properties()
        {
            Assert.AreEqual(_ellipsoidTissueRegion.Center.X, 0.0);
            Assert.AreEqual(_ellipsoidTissueRegion.Center.Y, 0.0);
            Assert.AreEqual(_ellipsoidTissueRegion.Center.Z, 3.0);
            Assert.AreEqual(_ellipsoidTissueRegion.Dx, 1.0);
            Assert.AreEqual(_ellipsoidTissueRegion.Dy, 1.0);
            Assert.AreEqual(_ellipsoidTissueRegion.Dz, 2.0);
        }
        /// <summary>
        /// Validate method OnBoundary return correct boolean
        /// </summary>
        [Test]
        public void verify_OnBoundary_method_returns_correct_result()
        {
            bool result = _ellipsoidTissueRegion.OnBoundary(new Position(0, 0, 1.0));
            Assert.IsTrue(result);
            result = _ellipsoidTissueRegion.OnBoundary(new Position(0, 0, 2.0));
            Assert.IsFalse(result);
        }
        /// <summary>
        /// Validate method SurfaceNormal return correct normal vector
        /// </summary>
        [Test]
        public void verify_SurfaceNormal_method_returns_correct_result()
        {
            Direction result = _ellipsoidTissueRegion.SurfaceNormal(new Position(0, 0, 1.0));
            Assert.AreEqual(new Direction(0, 0, -1), result);
            result = _ellipsoidTissueRegion.SurfaceNormal(new Position(0, 0, 5.0));
            Assert.AreEqual(new Direction(0, 0, 1), result);
        }
        /// <summary>
        /// Validate method RayIntersectBoundary return correct result
        /// </summary>
        [Test]
        public void verify_RayIntersectBoundary_method_returns_correct_result()
        {
            Photon photon = new Photon();
            photon.DP.Position = new Position(-2, 0, 3);
            photon.DP.Direction = new Direction(1, 0, 0);
            photon.S = 2.0; // definitely intersect
            double distanceToBoundary;
            bool result = _ellipsoidTissueRegion.RayIntersectBoundary(photon, out distanceToBoundary);
            Assert.AreEqual(true, result);
            Assert.AreEqual(1.0, distanceToBoundary);
            photon.S = 0.5; // definitely don't intersect
            result = _ellipsoidTissueRegion.RayIntersectBoundary(photon, out distanceToBoundary);
            Assert.AreEqual(false, result);
            Assert.AreEqual(Double.PositiveInfinity, distanceToBoundary);
            photon.S = 1.0; // ends right at boundary => both out and no intersection
            result = _ellipsoidTissueRegion.RayIntersectBoundary(photon, out distanceToBoundary);
            Assert.AreEqual(false, result);
            Assert.AreEqual(Double.PositiveInfinity, distanceToBoundary);
        }
    }
}
