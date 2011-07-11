using Vts.Common;

namespace Vts.MonteCarlo.Sources
{   
    // todo: re-do this file for new sources

    /// <summary>
    /// Implements ISourceInput.  Defines input data for  Isotropic PointSource implementation
    /// including position.
    /// </summary>
    public class IsotropicPointSourceInput : ISourceInput
    {
        // this handles point
        public IsotropicPointSourceInput(
            Position pointLocation,
            int initialTissueRegionIndex) 
        {
            SourceType = SourceType.IsotropicPoint;
            PointLocation = pointLocation;
            InitialTissueRegionIndex = initialTissueRegionIndex;            
        }

        public IsotropicPointSourceInput()
            : this(
                new Position (0, 0, 0),
                0) { }

        public Position PointLocation { get; set; }
        public SourceType SourceType { get; set; }
        public int InitialTissueRegionIndex { get; set; }
    }
}
