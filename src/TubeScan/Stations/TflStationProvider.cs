﻿using Tk.Extensions;
using Tk.Extensions.Time;
using TubeScan.Lines;
using TubeScan.Models;
using TubeScan.Tfl;

namespace TubeScan.Stations
{
    internal class TflStationProvider : IStationProvider
    {
        private readonly ITflClient _tflClient;
        private readonly ILineProvider _lineProvider;
        private readonly ITimeProvider _time;

        public TflStationProvider(ITflClient tflClient, ILineProvider lineProvider, ITimeProvider time)
        {
            _tflClient = tflClient;
            _lineProvider = lineProvider;
            _time = time;
        }
                
        public async Task<IList<Station>> GetStationsAsync()
        {
            var lines = await _lineProvider.GetLinesAsync();

            var tasks = lines.Select(l => GetStationsAsync(l.Id))
                             .ToArray();

            var data = await Task.WhenAll(tasks);

            return data.SelectMany(x => x).DistinctBy(s => s.NaptanId).ToList();
        }

        public async Task<StationStatus> GetStationStatusAsync(string naptanId)
        {           
            var now = _time.UtcNow();
            
            var liveCrowding = await GetLiveStationCrowdingAsync(naptanId);
            var avgCrowding = await GetAverageStationCrowdingAsync(naptanId, now);
            var arrivals = await GetStationArrivalsAsync(naptanId);

            var result = new StationStatus(naptanId)
            {
                Crowding = new StationCrowding()
                {
                    LivePercentageOfBaseline = liveCrowding,
                    AveragePercentageOfBaseline = avgCrowding,
                },
                Arrivals = arrivals
            };

            return result;
        }

        private async Task<IList<Station>> GetStationsAsync(string lineId)
        {
            var path = $"Line/{lineId}/StopPoints";
            var resp = await _tflClient.GetAsync(path, false);
            if (!resp.IsSuccess)
            {
                throw new ApplicationException($"Bad response from TFL: {resp.HttpStatus} received.");
            }

            return resp.Body.ToStations();
        }

        private async Task<double?> GetLiveStationCrowdingAsync(string naptanId)
        {
            var path = $"crowding/{naptanId}/Live";
            var resp = await _tflClient.GetAsync(path, true);
            if (!resp.IsSuccess)
            {
                throw new ApplicationException($"Bad response from TFL: {resp.HttpStatus} received.");
            }

            var j = Newtonsoft.Json.Linq.JToken.Parse(resp.Body);

            if (j.Value<bool>("dataAvailable"))
            {
                return j.Value<double>("percentageOfBaseline");
            }
            return null;
        }


        private async Task<double?> GetAverageStationCrowdingAsync(string naptanId, DateTime now)
        {                        
            var path = $"crowding/{naptanId}/{now.DayOfWeek.ToString()[..3]}";
            var resp = await _tflClient.GetAsync(path, true);
            if (!resp.IsSuccess)
            {
                throw new ApplicationException($"Bad response from TFL: {resp.HttpStatus} received.");
            }

            var result = resp.Body.ToTflDayOfWeekStationCrowding();
            
            var tod = now.TimeOfDay;
            
            var match = result.TimeBands.NullToEmpty().FirstOrDefault(tb => tod >= tb.From && tod < tb.To);

            return match?.PercentageOfBaseLine;
        }

        public async Task<IList<Arrival>> GetStationArrivalsAsync(string naptanId)
        {            
            var path = $"StopPoint/{naptanId}/Arrivals";
            var resp = await _tflClient.GetAsync(path, true);
            if (!resp.IsSuccess)
            {
                throw new ApplicationException($"Bad response from TFL: {resp.HttpStatus} received.");
            }

            return resp.Body.ToArrivals();
        }
    }
}
