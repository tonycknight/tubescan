using TubeScan.Models;

namespace TubeScan.Lines
{
    internal interface ILineReferenceProvider
    {
        IList<Line> GetLines();
    }
}
