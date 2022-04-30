using TubeScan.Models;

namespace TubeScan.Lines
{
    internal interface ILineProvider
    {
        Task<IList<Line>> GetLinesAsync();
        Task<IList<LineStatus>> GetLineStatusAsync();
    }
}
