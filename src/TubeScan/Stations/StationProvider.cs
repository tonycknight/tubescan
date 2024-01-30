using System.Runtime.Caching;
using Tk.Extensions.Funcs;
using Tk.Extensions.Guards;
using Tk.Extensions.Tasks;
using Tk.Extensions.Time;
using TubeScan.Models;
using TubeScan.Telemetry;

namespace TubeScan.Stations
{
    internal class StationProvider : IStationProvider, ISettable<Station>, ISettable<StationStatus>
    {
        private Lazy<Task<IList<Station>>> _stations;
        private readonly ITelemetry _telemetry;
        private readonly IStationProvider _sourceProvider;
        private readonly ITimeProvider _timeProvider;
        private readonly MemoryCache _stationStatusCache;

        public StationProvider(ITelemetry telemetry, TflStationProvider sourceProvider, ITimeProvider timeProvider)
        {
            _stations = new Lazy<Task<IList<Station>>>(() => GetStationsFromSourceAsync(sourceProvider));
            _telemetry = telemetry;
            _sourceProvider = sourceProvider;
            _timeProvider = timeProvider;
            _stationStatusCache = new MemoryCache("StationStatuses");
        }

        public Task<IList<Station>> GetStationsAsync() => _stations.Value;

        public async Task<StationStatus> GetStationStatusAsync(string naptanId)
        {
            var result = _stationStatusCache.Get(naptanId) as StationStatus;
            if (result == null)
            {
                result = await _sourceProvider.GetStationStatusAsync(naptanId);
                if (result != null)
                {
                    ((ISettable<StationStatus>)this).Set(new[] { result });
                }
            }

            return result;
        }

        void ISettable<Station>.Set(IList<Station> values)
        {
            _stations = values.ArgNotNull(nameof(values))
                              .Pipe(v => new Lazy<Task<IList<Station>>>(v.ToTaskResult()));
        }

        private static async Task<IList<Station>> GetStationsFromSourceAsync(IStationProvider sourceProvider)
            => await sourceProvider.GetStationsAsync();

        void ISettable<StationStatus>.Set(IList<StationStatus> values)
        {
            var now = _timeProvider.UtcNow();
            var policy = new CacheItemPolicy()
            {
                AbsoluteExpiration = now.AddMinutes(1),
                RemovedCallback = OnStationStatusCacheExpiry,
            };

            var items = values.Select(v => new CacheItem(v.NaptanId, v)).ToList();
            foreach (var value in values)
            {
                _stationStatusCache.Add(value.NaptanId, value, policy);
            }
        }

        private async void OnStationStatusCacheExpiry(CacheEntryRemovedArguments arguments)
        {
            var status = arguments.CacheItem.Value as StationStatus;
            if (status != null)
            {
                $"Station status {status.NaptanId} expired, refreshing...".CreateTelemetryEvent(TelemetryEventKind.Info).Send(_telemetry);
                try
                {
                    var newStatus = await _sourceProvider.GetStationStatusAsync(status.NaptanId);
                    ((ISettable<StationStatus>)this).Set(new[] { newStatus });

                    $"Station status {status.NaptanId} refreshed.".CreateTelemetryEvent(TelemetryEventKind.Info).Send(_telemetry);
                }
                catch (Exception ex)
                {
                    ex.Message.CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                }
            }
        }
    }
}
