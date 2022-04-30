namespace TubeScan.Io
{
    public interface IIoProvider
    {
        public StreamReader OpenFileReader(string filePath);

        public System.Collections.IDictionary GetEnvironmentVariables();
    }
}
