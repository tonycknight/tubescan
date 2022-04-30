using TubeScan.Models;

namespace TubeScan.Stations
{
    internal class StationProvider : IStationProvider
    {
        private readonly Lazy<IList<Station>> _stations;
        private readonly IStationProvider _sourceProvider;

        public StationProvider(TflStationProvider sourceProvider)
        {
            _stations = new Lazy<IList<Station>>(() => GetStations(sourceProvider));
            _sourceProvider = sourceProvider;
        }

        public Task<IList<Station>> GetStationsAsync()
            => _stations.Value.ToTaskResult();

        public Task<StationStatus> GetStationStatusAsync(string naptanId)
            => _sourceProvider.GetStationStatusAsync(naptanId);

        private static IList<Station> GetStations(IStationProvider sourceProvider) 
            => sourceProvider.GetStationsAsync().GetAwaiter().GetResult();
    }
}
