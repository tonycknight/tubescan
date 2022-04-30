using TubeScan.Models;

namespace TubeScan.Stations
{
    internal interface IStationProvider
    {
        Task<IList<Station>> GetStationsAsync();
        Task<StationStatus> GetStationStatusAsync(string naptanId);
    }
}
