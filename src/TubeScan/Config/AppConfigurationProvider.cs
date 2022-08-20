using System.Diagnostics.CodeAnalysis;

namespace TubeScan.Config
{
    [ExcludeFromCodeCoverage]
    internal class AppConfigurationProvider : IFileAppConfigurationProvider
    {
        private readonly FileAppConfigurationProvider _fileProvider;
        private readonly EnvVarAppConfigurationProvider _envVarProvider;
        private string _filePath;

        public AppConfigurationProvider(FileAppConfigurationProvider fileProvider, EnvVarAppConfigurationProvider envVarProvider)
        {
            _fileProvider = fileProvider;
            _envVarProvider = envVarProvider;
        }

        public AppConfiguration GetAppConfiguration()
        {
            if (_filePath != null)
            {
                return _fileProvider.SetFilePath(_filePath)
                                    .GetAppConfiguration();
            }
            return _envVarProvider.GetAppConfiguration();
        }

        public IFileAppConfigurationProvider SetFilePath(string filePath)
        {
            _filePath = filePath;
            return this;
        }
    }
}
