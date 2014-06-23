using System.Collections.Generic;
using System.Linq;
using Vts.MonteCarlo.PhotonData;
using Vts.MonteCarlo.Detectors;

namespace Vts.MonteCarlo.Controllers
{
    ///// <summary>
    ///// Controller for detectors.
    ///// </summary>
    //public class HistoryDetectorController : IDetectorController
    //{
    //    private IList<IDetector> _detectors;
    //    private ITissue _tissue;
    //    /// <summary>
    //    /// controller for history type detectors
    //    /// </summary>
    //    /// <param name="detectors">IEnumerable of IHistoryDetector</param>
    //    /// <param name="tissue">ITissue</param>
    //    public HistoryDetectorController(IEnumerable<IDetector> detectors, ITissue tissue)
    //    {
    //        //_detectors = (from d in detectors
    //        //              where d is IHistoryDetector
    //        //              select d).ToList();
    //        _detectors = detectors.ToList();
    //        _tissue = tissue;
    //    }
    //    /// <summary>
    //    /// Ilist of IDetector
    //    /// </summary>
    //    public IList<IDetector> Detectors { get { return _detectors.Select(d => (IDetector)d).ToList(); } }
    //    /// <summary>
    //    /// method to tally to detectors managed by this controller
    //    /// </summary>
    //    /// <param name="photon">Photon</param>
    //    public void Tally(Photon photon)
    //    {
    //        //PhotonDataPoint previousDP = photon.History.HistoryData.First();
    //        //foreach (PhotonDataPoint dp in photon.History.HistoryData.Skip(1))
    //        //{
    //        //    var currentRegionIndex = _tissue.GetRegionIndex(dp.Position);
    //        //    foreach (var detector in _detectors)
    //        //    {
    //        //        ((IDetector)detector).TallySingle(previousDP, dp, currentRegionIndex);
    //        //        //previousDP = dp;
    //        //    }
    //        //    previousDP = dp;
    //        //}
    //        // individual history detectors will call TallySingle on their own
    //        foreach (var detector in _detectors)
    //        {
    //            detector.Tally(photon);
    //        }
    //    }
    //    /// <summary>
    //    /// method to normalize detector tallies
    //    /// </summary>
    //    /// <param name="N">number of photons launched from source</param>
    //    public virtual void NormalizeDetectors(long N)
    //    {
    //        foreach (var detector in _detectors)
    //        {
    //            detector.Normalize(N);
    //        }
    //    }
    //}
}
