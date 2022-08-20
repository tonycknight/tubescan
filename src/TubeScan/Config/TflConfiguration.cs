using Newtonsoft.Json;

namespace TubeScan.Config
{
    internal class TflConfiguration
    {

        [JsonProperty("tflUri")]
        public string? TflUri { get; set; } = "https://api.tfl.gov.uk";

        [JsonProperty("appKey")]
        public string? AppKey { get; set; }
    }
}
