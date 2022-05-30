using TubeScan.Scheduling;
using TubeScan.Telemetry;

namespace TubeScan.Stations
{
    internal class StationCacheRefreshJob : IJob
    {
        private readonly ITelemetry _telemetry;
                
        public StationCacheRefreshJob(ITelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        public TimeSpan Frequency => TimeSpan.FromSeconds(5); // TODO: 

        public Task ExecuteAsync()
        {

            return Task.CompletedTask;
        }
    }
}
