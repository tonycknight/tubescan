using TubeScan.Models;

namespace TubeScan.Lines
{
    internal interface ILineStatusProvider
    {
        Task<IList<LineStatus>> GetLineStatusAsync();
    }
}
