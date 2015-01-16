using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Detectors;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.Detectors
{
    /// <summary>
    /// These tests execute an MC simulation with 100 photons and verify
    /// that all photons tallie to specular
    /// </summary>
    [TestFixture]
    public class FluenceTetrahedralMeshDetectorTests
    {
        private SimulationOutput _output;

        /// <summary>
        /// Setup input to the MC, SimulationInput, and execute MC
        /// </summary>
        [TestFixtureSetUp]
        public void execute_Monte_Carlo()
        {
            // define vertices of 6 tetrahedra within cube spanning [-10,10]x[10,10]x[0,20]mm
            var vertex1 = new Position(10, 10, 0);
            var vertex2 = new Position(10, -10, 0);
            var vertex3 = new Position(-10, -10, 0);
            var vertex4 = new Position(-10, 10, 0);
            var vertex5 = new Position(10, 10, 20);
            var vertex6 = new Position(10, -10, 20);
            var vertex7 = new Position(-10, -10, 20);
            var vertex8 = new Position(-10, 10, 20);
            var input = new SimulationInput(
                 100,
                 "Output",
                 new SimulationOptions(
                     0,
                     RandomNumberGeneratorType.MersenneTwister,
                     AbsorptionWeightingType.Analog,
                     PhaseFunctionType.HenyeyGreenstein,
                     new List<DatabaseType>() { }, // databases to be written
                     false, // track statistics
                     0.0, // RR threshold -> 0 = no RR performed
                     0),
                 new DirectionalPointSourceInput( // this is right on boundary of tetra, may need to move
                     new Position(0.0, 0.0, 0.0),
                     new Direction(0.0, 0.0, 1.0),
                     0 // start in air
                 ),
                 new MultiTetrahedronInCubeTissueInput(
                     new ITissueRegion[]
                    { 
                        new TetrahedronTissueRegion( new Position[] { vertex1, vertex2, vertex3, vertex6 }, 
                            new OpticalProperties(0.01, 1, 0.8, 1.4)), 
                        new TetrahedronTissueRegion( new Position[] { vertex1, vertex3, vertex4, vertex8 }, 
                            new OpticalProperties(0.01, 1, 0.8, 1.4)),
                        new TetrahedronTissueRegion( new Position[] { vertex1, vertex3, vertex6, vertex8 }, 
                            new OpticalProperties(0.01, 1, 0.8, 1.4)),
                        new TetrahedronTissueRegion( new Position[] { vertex1, vertex5, vertex6, vertex8 }, 
                            new OpticalProperties(0.01, 1, 0.8, 1.4)),
                        new TetrahedronTissueRegion(new Position[] { vertex3, vertex6, vertex7, vertex8 }, 
                            new OpticalProperties(0.01, 1, 0.8, 1.4)),
                    },
                    ""
                 ),
                new List<IDetectorInput>
                {
                    new ATotalDetectorInput(),
                    new FluenceOfTetrahedralMeshDetectorInput(), 
                }
            );
            _output = new MonteCarloSimulation(input).Run();
        }
        
        // Validate absorbed energy
        [Test]
        public void validate_ATotal()
        {
            Assert.Less(Math.Abs(_output.Atot - 0.001), 0.003);
        }
    }
}
