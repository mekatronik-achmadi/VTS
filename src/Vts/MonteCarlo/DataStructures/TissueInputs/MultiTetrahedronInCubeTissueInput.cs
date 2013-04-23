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
    [KnownType(typeof(OpticalProperties))]
    public class MultiTetrahedronInCubeTissueInput : ITissueInput
    {
        private ITissueRegion[] _regions;

        /// <summary>
        /// constructor for Multi-tetrahedron in cube tissue input
        /// </summary>
        /// <param name="regions">list of tissue regions comprising tissue</param>
        public MultiTetrahedronInCubeTissueInput(ITissueRegion[] regions)
        {
            _regions = regions;
        }

        /// <summary>
        /// MultiTetrahedronInCubeTissue default constructor provides homogeneous tissue
        /// </summary>
        public MultiTetrahedronInCubeTissueInput()
            : this(
                new ITissueRegion[]
                { 
                    // need to determine how to bring in list of tetrahedra
                    new TetrahedronRegion(
                        new List<Position>()
                            {
                                new Position(0, 0, 0),
                                new Position(0, 0, 1),
                                new Position(0, 1, 0),
                                new Position(1, 0, 0),                                
                            }, 
                        new OpticalProperties( 0.01, 1, 0.8, 1.4))
                })
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
    }
}
