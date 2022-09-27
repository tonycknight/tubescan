using Tk.Extensions.Funcs;
using Tk.Extensions.Guards;
using Tk.Extensions.Tasks;
using TubeScan.Models;

namespace TubeScan.Stations
{
    internal class StationProvider : IStationProvider, ISettable<Station>
    {
        private Lazy<Task<IList<Station>>> _stations;
        private readonly IStationProvider _sourceProvider;

        public StationProvider(TflStationProvider sourceProvider)
        {
            _stations = new Lazy<Task<IList<Station>>>(() => GetStationsFromSourceAsync(sourceProvider));
            _sourceProvider = sourceProvider;
        }

        public Task<IList<Station>> GetStationsAsync()
            => _stations.Value;

        public Task<StationStatus> GetStationStatusAsync(string naptanId)
            => _sourceProvider.GetStationStatusAsync(naptanId);

        void ISettable<Station>.Set(IList<Station> values)
        {
            _stations = values.ArgNotNull(nameof(values))
                              .Pipe(v => new Lazy<Task<IList<Station>>>(v.ToTaskResult()));
        }

        private static async Task<IList<Station>> GetStationsFromSourceAsync(IStationProvider sourceProvider) 
            => await sourceProvider.GetStationsAsync();

        
    }
}
