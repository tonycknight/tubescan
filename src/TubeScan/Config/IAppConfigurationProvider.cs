namespace TubeScan.Config
{
    internal interface IAppConfigurationProvider
    {
        public IAppConfigurationProvider SetFilePath(string filePath);

        public AppConfiguration GetAppConfiguration();
    }
}
