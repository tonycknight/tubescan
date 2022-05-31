using Tk.Extensions;
using Tk.Extensions.Tasks;
using TubeScan.Models;

namespace TubeScan.Stations
{
    internal class StationProvider : IStationProvider, ISettable<Station>
    {
        private Lazy<IList<Station>> _stations;
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

        void ISettable<Station>.Set(IList<Station> values)
        {
            _stations = values.ArgNotNull(nameof(values))
                              .Pipe(v => new Lazy<IList<Station>>(v));
        }

        private static IList<Station> GetStations(IStationProvider sourceProvider) 
            => sourceProvider.GetStationsAsync().GetAwaiter().GetResult();

        
    }
}
