using Newtonsoft.Json;

namespace TubeScan.Config
{
    internal class FileAppConfigurationProvider : IAppConfigurationProvider
    {
        private readonly Io.IIoProvider _ioProvider;
        private string? _filePath;

        public FileAppConfigurationProvider(Io.IIoProvider ioProvider)
        {
            _ioProvider = ioProvider;
        }

        public AppConfiguration GetAppConfiguration()
        {
            _filePath.InvalidOpArg(p => p == null, "File path not set.");

            return GetAppConfiguration(_filePath);
        }

        public IAppConfigurationProvider SetFilePath(string filePath)
        {
            _filePath = filePath.ArgNotNull(nameof(filePath));
            return this;
        }

        private AppConfiguration GetAppConfiguration(string filePath)
        {
            try
            {
                using var sRdr = _ioProvider.OpenFileReader(filePath);
                using var jRdr = new JsonTextReader(sRdr);

                var s = JsonSerializer.Create();

                return s.Deserialize<AppConfiguration>(jRdr);
            }
            catch (JsonSerializationException)
            {
                throw new InvalidOperationException("The config file is not valid JSON.");
            }
            catch (JsonReaderException)
            {
                throw new InvalidOperationException("The config file is not valid JSON.");
            }
        }
    }
}
