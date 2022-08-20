using System.Diagnostics.CodeAnalysis;

namespace TubeScan.Config
{
    [ExcludeFromCodeCoverage]
    internal class AppConfigurationProvider : IFileAppConfigurationProvider
    {
        private readonly FileAppConfigurationProvider _fileProvider;
        private readonly EnvVarAppConfigurationProvider _envVarProvider;
        private string _filePath;
        private readonly Lazy<AppConfiguration> _getConfig;

        public AppConfigurationProvider(FileAppConfigurationProvider fileProvider, EnvVarAppConfigurationProvider envVarProvider)
        {
            _fileProvider = fileProvider;
            _envVarProvider = envVarProvider;
            _getConfig = new Lazy<AppConfiguration>(() => FetchAppConfiguration());
        }

        public AppConfiguration GetAppConfiguration() => _getConfig.Value;

        public IFileAppConfigurationProvider SetFilePath(string filePath)
        {
            _filePath = filePath;
            return this;
        }

        private AppConfiguration FetchAppConfiguration()
        {
            if (_filePath != null)
            {
                return _fileProvider.SetFilePath(_filePath)
                                    .GetAppConfiguration();
            }
            return _envVarProvider.GetAppConfiguration();
        }
    }
}
