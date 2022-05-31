using Newtonsoft.Json;

namespace TubeScan.Stations
{
    internal class TflStationLine
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    internal class TflStation
    {
        [JsonProperty("naptanId")]
        public string NaptanId { get; set; }

        [JsonProperty("commonName")]
        public string CommonName { get; set; }

        [JsonProperty("lines")]
        public TflStationLine[] Lines { get; set; }
    }


    internal class TflDayOfWeekStationCrowding
    {
        [JsonProperty("naptan")]
        public string NaptanId { get; set; }

        [JsonProperty("timeBands")]
        public IList<TflTimeBandStationCrowding> TimeBands { get; set; }
    }

    internal class TflTimeBandStationCrowding
    {
        [JsonProperty("timeBand")]
        public string TimeBand { get; set; }

        [JsonIgnore]
        public TimeSpan From
        {
            get
            {
                var idx = TimeBand.IndexOf("-");
                return TimeSpan.Parse(TimeBand.Substring(0, idx));
            }
        }

        [JsonIgnore]
        public TimeSpan To
        {
            get
            {
                var idx = TimeBand.IndexOf("-");
                return TimeSpan.Parse(TimeBand.Substring(idx + 1));
            }
        }

        [JsonProperty("percentageOfBaseLine")]
        public double PercentageOfBaseLine { get; set; }
    }

    internal class TflArrivalPrediction
    {
        [JsonProperty("lineId")]
        public string LineId { get; set; }

        [JsonProperty("vehicleId")]
        public string VehicleId { get; set; }

        [JsonProperty("destinationNaptanId")]
        public string DestinationNaptanId { get; set; }

        [JsonProperty("currentLocation")]
        public string CurrentLocation { get; set; }

        [JsonProperty("platformName")]
        public string PlatformName { get; set; }

        [JsonProperty("expectedArrival")]
        public DateTimeOffset ExpectedArrival { get; set; }
    }
}
