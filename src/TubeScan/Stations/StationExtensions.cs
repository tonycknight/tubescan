using Tk.Extensions.Linq;
using TubeScan.Models;

namespace TubeScan.Stations
{
    internal static class StationExtensions
    {
        public static IList<Station> ToStations(this string json)
            => Newtonsoft.Json.JsonConvert.DeserializeObject<TflStation[]>(json)
                                          .Select(ToStation)
                                          .ToList();

        public static Station ToStation(TflStation value)
        {
            var lines = value.Lines.NullToEmpty()
                .Select(tsl => new StationLine(tsl.Id, tsl.Name))
                .ToList();
            return new Station(value.NaptanId, value.CommonName, lines);
        }

        public static TflDayOfWeekStationCrowding ToTflDayOfWeekStationCrowding(this string json)
            => Newtonsoft.Json.JsonConvert.DeserializeObject<TflDayOfWeekStationCrowding>(json);


        public static IEnumerable<Arrival> ToArrivals(this string json)
            => Newtonsoft.Json.JsonConvert.DeserializeObject<TflArrivalPrediction[]>(json)
                                          .Select(ToArrival);

        public static Arrival ToArrival(TflArrivalPrediction value)
            => new Arrival()
            {
                LineId = value.LineId,
                VehicleId = value.VehicleId,
                ArrivalPlatform = value.PlatformName,
                CurrentLocation = value.CurrentLocation,
                DestinationId = value.DestinationNaptanId,
                ExpectedArrival = value.ExpectedArrival,
            };

        public static DateTimeOffset? GetResponseDate(this IDictionary<string, string[]> headers)
        {
            if (headers?.TryGetValue("Date", out var values) == true)
            {
                var val = values.FirstOrDefault();
                if (val != null && DateTimeOffset.TryParse(val, out var result))
                {
                    return result;
                }
            }

            return default;
        }

        public static Arrival ApplyExpectedWait(this Arrival value, DateTimeOffset? serverTime)
        {
            if (serverTime != default)
            {
                var wait = value.ExpectedArrival - serverTime;
                if (wait >= TimeSpan.Zero)
                {
                    value.ExpectedWait = wait;
                }
            }
            return value;
        }
    }
}
