using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Vts.Common;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo
{
    /// <summary>
    /// Implements ITissueInput.  Defines input to MultiTetrahedronInCubeTissue class.
    /// </summary>
    [KnownType(typeof(TetrahedronRegion))]
    [KnownType(typeof(TriangleRegion))]
    [KnownType(typeof(OpticalProperties))]
    [KnownType(typeof(string))]
    public class MultiTetrahedronInCubeTissueInput : ITissueInput
    {
        private ITissueRegion[] _regions;
        private string _meshDataFilename;

        /// <summary>
        /// constructor for Multi-tetrahedron in cube tissue input
        /// </summary>
        /// <param name="regions">list of tissue regions comprising tissue</param>
        // Question: ITissue expects an array of ITissueRegion.  Can I use filename 
        // to instantiate Regions?
        public MultiTetrahedronInCubeTissueInput(ITissueRegion[] regions, string meshDataFilename)
        {
            _regions = regions;
            _meshDataFilename = meshDataFilename;
        }

        /// <summary>
        /// MultiTetrahedronInCubeTissue default constructor provides homogeneous tissue
        /// </summary>
        public MultiTetrahedronInCubeTissueInput()
            : this(
                new ITissueRegion[]
                { 
                    // need to determine how to bring in list of tetrahedra
                    new TetrahedronRegion()
                },
        "cube")
        {
        }
        /// <summary>
        /// tissue identifier
        /// </summary>
        [IgnoreDataMember]
        public TissueType TissueType { get { return TissueType.MultiTetrahedronInCube; } }
        /// <summary>
        /// list of tissue regions comprising tissue
        /// </summary>
        public ITissueRegion[] Regions { get { return _regions; } set { _regions = value; } }
        /// <summary>
        /// filename of input mesh data
        /// </summary>
        public string MeshDataFilename { get { return _meshDataFilename; } set { _meshDataFilename = value; } }
    }
}
