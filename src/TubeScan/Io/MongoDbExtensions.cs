using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;

namespace TubeScan.Io
{
    internal static class MongoDbExtensions
    {
        [ExcludeFromCodeCoverage]
        public static void RecreateIndex<T>(this IMongoCollection<T> col, string name, Action<string, IMongoCollection<T>> create)
        {
            try
            {
                create(name, col);
            }
            catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict" || ex.CodeName == "IndexKeySpecsConflict")
            {
                col.Indexes.DropOne(name);
                create(name, col);
            }
        }

        [ExcludeFromCodeCoverage]
        public static IMongoDatabase GetDb(this Config.MongoConfiguration config)
        {
            var isLocalhost = config.Connection.Contains("localhost");

            var settings = MongoClientSettings.FromConnectionString(config.Connection);

            settings.AllowInsecureTls = isLocalhost;
            settings.UseTls = !isLocalhost;
            settings.ConnectTimeout = TimeSpan.FromSeconds(15);
            settings.ServerSelectionTimeout = settings.ConnectTimeout;

            var client = new MongoClient(settings);

            return client.GetDatabase(config.DatabaseName);
        }
    }
}
