using Newtonsoft.Json;

namespace TubeScan.Config
{
    internal class DiscordConfiguration
    {
        [JsonProperty("clientId")]
        public string? ClientId { get; set; }

        [JsonProperty("clientToken")]
        public string? ClientToken { get; set; }
    }
}
