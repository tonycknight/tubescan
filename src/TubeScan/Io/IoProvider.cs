using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace TubeScan.Io
{
    [ExcludeFromCodeCoverage]
    public class IoProvider : IIoProvider
    {
        public IDictionary GetEnvironmentVariables() => System.Environment.GetEnvironmentVariables();

        public StreamReader OpenFileReader(string filePath) => File.OpenText(filePath);
    }
}
