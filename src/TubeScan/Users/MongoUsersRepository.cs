using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;
using TubeScan.Io;
using TubeScan.Models;

namespace TubeScan.Users
{
    [ExcludeFromCodeCoverage]
    internal class MongoUsersRepository : IUsersRepository
    {
        private readonly Lazy<IMongoCollection<UserDto>> _usersCol;

        public MongoUsersRepository(Config.IAppConfigurationProvider config)
        {
            _usersCol = new Lazy<IMongoCollection<UserDto>>(() => InitialiseDb(config));
        }

        public async Task<IList<User>> GetAllUsersAsync()
        {
            var col = _usersCol.Value;

            var filter = FilterDefinition<UserDto>.Empty;

            var result = (await col.FindAsync(filter)).ToEnumerable();

            return result.Select(x => x.FromDto()).ToList();
        }

        public Task SetUserAsync(User value)
        {
            var col = _usersCol.Value;
            var filter = CreateEqualityFilter(value.Id);

            var update = Builders<UserDto>.Update.Set(u => u.UserId, value.Id)
                                                 .Set(u => u.Mention, value.Mention);

            return col.UpdateOneAsync(filter, update, new UpdateOptions() { IsUpsert = true });
        }

        private IMongoCollection<UserDto> InitialiseDb(Config.IAppConfigurationProvider configProvider)
        {
            var config = configProvider.GetAppConfiguration();

            var db = config.Mongo.GetDb();

            return CreateUsersCollection(config.Mongo, db);
        }

        private IMongoCollection<UserDto> CreateUsersCollection(Config.MongoConfiguration config, IMongoDatabase db)
        {
            var colName = config.UsersCollectionName;
            var col = db.GetCollection<UserDto>(colName);
            if (col == null)
            {
                var opts = new CreateCollectionOptions();
                db.CreateCollection(colName, opts);
            }
            col = db.GetCollection<UserDto>(colName);

            col.RecreateIndex("unique", (n, c) => CreateUniqueIndex(n, col));

            return col;
        }

        private void CreateUniqueIndex(string name, IMongoCollection<UserDto> col)
        {
            var build = Builders<UserDto>.IndexKeys;

            var uniqueIndexModel = new CreateIndexModel<UserDto>(
                    build.Ascending(x => x.UserId),
                    new CreateIndexOptions() { Name = name, Unique = true, Background = false });

            col.Indexes.CreateOne(uniqueIndexModel);
        }

        private FilterDefinition<UserDto> CreateEqualityFilter(ulong userId)
            => Builders<UserDto>.Filter.And(
                            Builders<UserDto>.Filter.Eq(us => us.UserId, userId));

    }
}
