using Newtonsoft.Json;

namespace TubeScan.Lines
{
    internal class TflLineStatusCheck
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lineStatuses")]
        public TflLineStatus[] LineStatuses { get; set; }
    }

    internal class TflLineStatus
    {
        [JsonProperty("statusSeverity")]
        public int StatusSeverity { get; set; }
        
        [JsonProperty("statusSeverityDescription")]
        public string StatusSeverityDescription { get; set; }
        
        [JsonProperty("reason")]
        public string Reason { get; set; }
        
        [JsonProperty("validityPeriods")]
        public TflValidityPeriod[] ValidityPeriods { get; set; }

        [JsonProperty("disruption")]
        public TflDisruption Disruption { get; set; }
    }

    internal class TflValidityPeriod
    {
        [JsonProperty("fromDate")]
        public DateTimeOffset From { get; set; }
        
        [JsonProperty("toDate")]
        public DateTimeOffset To { get; set; }

        [JsonProperty("isNow")]
        public bool? IsNow { get; set; }
    }

    internal class TflDisruption
    {
        [JsonProperty("affectedStops")]
        public TflAffectedStop[] AffectedStops { get; set; }
    }

    internal class TflAffectedStop
    {
        [JsonProperty("naptanId")]
        public string NaptanId { get; set; }

        [JsonProperty("stationNaptanId")]
        public string StationNaptanId { get; set; }

        [JsonProperty("commonName")]
        public string Name { get; set; }
    }
}
