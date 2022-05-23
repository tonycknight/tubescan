using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TubeScan.Users
{
    internal class UserDto
    {
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        public ulong UserId { get; set; }

        [BsonElement("mention")]
        public string Mention { get; set; }
    }
}
