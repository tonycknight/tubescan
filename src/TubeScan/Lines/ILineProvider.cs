using TubeScan.Models;

namespace TubeScan.Lines
{
    internal interface ILineProvider : ILineStatusProvider
    {
        Task<IList<Line>> GetLinesAsync();
    }
}
