using Newtonsoft.Json;

namespace TubeScan.Config
{
    internal class TflConfiguration
    {

        [JsonProperty("appKey")]
        public string? AppKey { get; set; }
    }
}
