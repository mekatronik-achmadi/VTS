using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.DataStructuresValidation.TissueInputs
{
    [TestFixture]
    public class MultiLayerTissueInputValidationTests
    {
        /// <summary>
        /// Test to check that layers that overlap.
        /// </summary>
        [Test]
        public void validate_MultiLayerTissue_layers_do_not_overlap()
        {
            var input = new SimulationInput(
                10,
                "",
                new SimulationOptions(),
                new DirectionalPointSourceInput(),
                new MultiLayerTissueInput(
                    new List<ITissueRegion>
                    { 
                        new LayerRegion(
                            new DoubleRange(double.NegativeInfinity, 0.0),
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                        new LayerRegion(
                            new DoubleRange(0.0, 50.0),
                            new OpticalProperties(0.01, 1.0, 0.8, 1.4)),
                        new LayerRegion(
                            new DoubleRange(20.0, double.PositiveInfinity),
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                    }
                ),
                new List<IVirtualBoundaryInput>
                    {
                        new SurfaceVirtualBoundaryInput(
                            VirtualBoundaryType.DiffuseReflectance,
                            new List<IDetectorInput>(){ },
                            false,
                            VirtualBoundaryType.DiffuseReflectance.ToString()),
                        new SurfaceVirtualBoundaryInput(
                            VirtualBoundaryType.DiffuseTransmittance,
                            new List<IDetectorInput>(){},
                            false,
                            VirtualBoundaryType.DiffuseTransmittance.ToString())
                    }
                );
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsFalse(result.IsValid);
        }
        /// <summary>
        /// Test to check that 0 thickness layers not defined.
        /// </summary>
        [Test]
        public void validate_MultiLayerTissue_layers_do_not_have_0_thickness()
        {
            var input = new SimulationInput(
                10,
                "",
                new SimulationOptions(),
                new DirectionalPointSourceInput(),
                new MultiLayerTissueInput(
                    new List<ITissueRegion>
                    { 
                        new LayerRegion(
                            new DoubleRange(double.NegativeInfinity, 0.0),
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                        new LayerRegion(
                            new DoubleRange(0.0, 0.0),
                            new OpticalProperties(0.01, 1.0, 0.8, 1.4)),
                        new LayerRegion(
                            new DoubleRange(0.0, double.PositiveInfinity),
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                    }
                ),
                new List<IVirtualBoundaryInput>
                    {
                        new SurfaceVirtualBoundaryInput(
                            VirtualBoundaryType.DiffuseReflectance,
                            new List<IDetectorInput>(){ },
                            false,
                            VirtualBoundaryType.DiffuseReflectance.ToString()),
                        new SurfaceVirtualBoundaryInput(
                            VirtualBoundaryType.DiffuseTransmittance,
                            new List<IDetectorInput>(){},
                            false,
                            VirtualBoundaryType.DiffuseTransmittance.ToString())
                    }
                );
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsFalse(result.IsValid);
        }
    }
}