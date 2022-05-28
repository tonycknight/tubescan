using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;
using TubeScan.Io;
using TubeScan.Models;

namespace TubeScan.Stations
{
    [ExcludeFromCodeCoverage]
    internal class MongoStationTagRepository : IStationTagRepository
    {
        private readonly Lazy<IMongoCollection<StationTagDto>> _tagsCol;

        public MongoStationTagRepository(Config.IAppConfigurationProvider config)
        {
            _tagsCol = new Lazy<IMongoCollection<StationTagDto>>(() => InitialiseDb(config));
        }

        public async Task<StationTag> GetAsync(ulong userId, string tag)
        {
            var col = _tagsCol.Value;
            var filter = CreateTagEqualityFilter(userId, tag);

            var result = (await col.FindAsync(filter)).FirstOrDefault();

            return result?.FromDto();
        }

        public async Task<IList<StationTag>> GetAllAsync(ulong userId)
        {            
            var col = _tagsCol.Value;
            var filter = CreateUserIdFilter(userId);

            var result = (await col.FindAsync(filter)).ToEnumerable();

            return result.Select(x => x.FromDto()).OrderBy(t => t.Tag).ToList();
        }

        public async Task RemoveAsync(ulong userId, string tag)
        {
            var col = _tagsCol.Value;
            var filter = CreateTagEqualityFilter(userId, tag);

            await col.DeleteOneAsync(filter);
        }

        public Task SetAsync(ulong userId, StationTag tag)
        {
            var col = _tagsCol.Value;
            var filter = CreateTagEqualityFilter(userId, tag.Tag);

            var update = Builders<StationTagDto>.Update.Set(st => st.UserId, userId)
                                                       .Set(st => st.Tag, tag.Tag.ToLower())
                                                       .Set(st => st.NaptanId, tag.NaptanId);

            return col.UpdateOneAsync(filter, update, new UpdateOptions() { IsUpsert = true });
        }

        private FilterDefinition<StationTagDto> CreateTagEqualityFilter(ulong userId, string tag)
            => Builders<StationTagDto>.Filter.And(
                            Builders<StationTagDto>.Filter.Eq(us => us.UserId, userId),
                            Builders<StationTagDto>.Filter.Eq(us => us.Tag, tag.ToLower()));

        private FilterDefinition<StationTagDto> CreateUserIdFilter(ulong userId)
            => Builders<StationTagDto>.Filter.And(
                            Builders<StationTagDto>.Filter.Eq(us => us.UserId, userId));

        private IMongoCollection<StationTagDto> InitialiseDb(Config.IAppConfigurationProvider configProvider)
        {
            var config = configProvider.GetAppConfiguration();

            var db = config.Mongo.GetDb();

            return CreateStationTagsCollection(config.Mongo, db);
        }

        private IMongoCollection<StationTagDto> CreateStationTagsCollection(Config.MongoConfiguration config, IMongoDatabase db)
        {
            var colName = config.StationTagsCollectionName;
            var col = db.GetCollection<StationTagDto>(colName);
            if (col == null)
            {
                var opts = new CreateCollectionOptions();
                db.CreateCollection(colName, opts);
            }
            col = db.GetCollection<StationTagDto>(colName);

            col.RecreateIndex("unique", (n, c) => CreateUniqueIndex(n, col));

            return col;
        }


        private void CreateUniqueIndex(string name, IMongoCollection<StationTagDto> col)
        {
            var build = Builders<StationTagDto>.IndexKeys;

            var uniqueIndexModel = new CreateIndexModel<StationTagDto>(
                    build.Ascending(x => x.UserId)
                         .Ascending(x => x.Tag),
                    new CreateIndexOptions() { Name = name, Unique = true, Background = false });

            col.Indexes.CreateOne(uniqueIndexModel);
        }
    }
}
