using System;
using System.Collections.Generic;
using Vts.Common;

namespace Vts.MonteCarlo
{
    /// <summary>
    /// DetectorInput for Flu(tetrahedra)
    /// </summary>
    public class FluenceOfTetrahedralMeshDetectorInput : IDetectorInput
    {
        /// <summary>
        /// constructor for fluence as a function of tetrahedrl mesh detector input.
        /// tetrahedral mesh defined by MultiTetrahedronInCubeTisssue so no input is needed here.
        /// SimulationInput validation will check that if this is in infile, MultiTetrahedronInCubeTissueInput
        /// must be also specified in infile
        /// Question: is this a good design?
        /// </summary>
        /// <param name="name">detector name</param>
        public FluenceOfTetrahedralMeshDetectorInput(
            String name)
        {
            TallyType = TallyType.FluenceOfTetrahedralMesh;
            Name = name;
        }

        /// <summary>
        /// Default constructor 
        /// </summary>
        public FluenceOfTetrahedralMeshDetectorInput() 
            : this(
                TallyType.FluenceOfTetrahedralMesh.ToString()) {}

        /// <summary>
        /// detector identifier
        /// </summary>
        public TallyType TallyType { get; set; }
        /// <summary>
        /// detector name
        /// </summary>
        public String Name { get; set; }
    }
}
