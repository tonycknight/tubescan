using TubeScan.Models;

namespace TubeScan.Stations
{
    internal interface IStationTagRepository
    {
        Task SetAsync(ulong userId, StationTag tag);
        Task<StationTag> GetAsync(ulong userId, string tag);
        Task<IList<StationTag>> GetAllAsync(ulong userId);

    }
}
