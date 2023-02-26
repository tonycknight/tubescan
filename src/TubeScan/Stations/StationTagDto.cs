using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TubeScan.Stations
{
    internal class StationTagDto
    {
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        public ulong UserId { get; set; }

        [BsonElement("tag")]
        public string Tag { get; init; }

        [BsonElement("naptanId")]
        public string NaptanId { get; init; }

    }
}
