using Tk.Extensions.Collections;

namespace TubeScan.Config
{
    internal class EnvVarAppConfigurationProvider : IAppConfigurationProvider
    {
        private const string EnvVarNamePrefix = "TubeScan";
        private readonly Io.IIoProvider _io;

        public EnvVarAppConfigurationProvider(Io.IIoProvider io)
        {
            _io = io;
        }

        public AppConfiguration GetAppConfiguration()
        {
            var result = CreateConfiguration();

            var evs = _io.GetEnvironmentVariables();
            if (evs != null)
            {
                var xs = evs.OfType<System.Collections.DictionaryEntry>()
                    .Select(ev => ((string)ev.Key, (string)ev.Value))
                    .Where(t => t.Item1.StartsWith(EnvVarNamePrefix, StringComparison.InvariantCulture))
                    .ToDictionary(t => t.Item1, t => t.Item2);

                result.Discord.ClientId = xs.GetOrDefault($"{EnvVarNamePrefix}_Discord_DiscordClientId") ?? result.Discord.ClientId;
                result.Discord.ClientToken = xs.GetOrDefault($"{EnvVarNamePrefix}_Discord_DiscordClientToken") ?? result.Discord.ClientToken;
                result.Mongo.Connection = xs.GetOrDefault($"{EnvVarNamePrefix}_Mongo_Connection") ?? result.Mongo.Connection;
                result.Mongo.DatabaseName = xs.GetOrDefault($"{EnvVarNamePrefix}_Mongo_DatabaseName") ?? result.Mongo.DatabaseName;
                result.Tfl.AppKey = xs.GetOrDefault($"{EnvVarNamePrefix}_Tfl_AppKey") ?? result.Tfl.AppKey;
            }

            return result;
        }

        public IAppConfigurationProvider SetFilePath(string filePath)
        {
            throw new NotImplementedException();
        }

        private AppConfiguration CreateConfiguration()
            => new AppConfiguration()
            {
                Discord = new DiscordConfiguration(),
                Tfl = new TflConfiguration(),
                Mongo = new MongoConfiguration(),
                Telemetry = new TelemetryConfiguration(),
            };
    }
}
