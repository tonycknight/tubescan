using TubeScan.Models;
using TubeScan.Scheduling;
using TubeScan.Telemetry;

namespace TubeScan.Stations
{
    internal class StationCacheRefreshJob : IJob
    {
        private readonly ITelemetry _telemetry;
        private readonly ISettable<Station> _stationSettable;
        private readonly IStationProvider _stationProvider;

        public StationCacheRefreshJob(ITelemetry telemetry, 
                                      ISettable<Models.Station> stationSettable,
                                      TflStationProvider stationProvider)
        {
            _telemetry = telemetry;
            _stationSettable = stationSettable;
            _stationProvider = stationProvider;
        }

        public TimeSpan Frequency => TimeSpan.FromMinutes(15);

        public async Task ExecuteAsync()
        {
            "Fetching stations...".CreateTelemetryEvent().Send(_telemetry);
            var stations = await _stationProvider.GetStationsAsync();
            $"Received {stations.Count} station(s)...".CreateTelemetryEvent().Send(_telemetry);

            if (stations?.Count > 0)
            {
                "Refreshing station cache...".CreateTelemetryEvent().Send(_telemetry);
                _stationSettable.Set(stations);
            }
        }
    }
}
