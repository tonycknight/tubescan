namespace TubeScan.Config
{
    internal interface IFileAppConfigurationProvider : IAppConfigurationProvider
    {
        public IFileAppConfigurationProvider SetFilePath(string filePath);
    }
}
