using Newtonsoft.Json;

namespace TubeScan.Config
{
    internal class AppConfiguration
    {
        [JsonProperty("discord")]
        public DiscordConfiguration? Discord { get; set; }

        [JsonProperty("mongo")]
        public MongoConfiguration? Mongo { get; set; }

        [JsonProperty("tfl")]
        public TflConfiguration? Tfl { get; set; }

        [JsonProperty("telemetry")]
        public TelemetryConfiguration Telemetry { get; set; }
    }
}
