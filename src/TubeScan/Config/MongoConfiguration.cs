using Newtonsoft.Json;

namespace TubeScan.Config
{
    internal class MongoConfiguration
    {
        [JsonProperty("connection")]
        public string? Connection { get; set; }

        [JsonProperty("databaseName")]
        public string? DatabaseName { get; set; } = "tubescan";

        [JsonProperty("stationTagsCollectionName")]
        public string? StationTagsCollectionName { get; set; } = "stationtags";

    }
}
