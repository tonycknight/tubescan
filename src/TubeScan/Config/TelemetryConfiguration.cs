using Newtonsoft.Json;

namespace TubeScan.Config
{
    public class TelemetryConfiguration
    {
        [JsonProperty("logMessageContent")]
        public bool LogMessageContent { get; set; } = false;
    }
}
