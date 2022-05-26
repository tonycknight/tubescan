namespace TubeScan.Tfl
{
    internal interface ITflClient
    {
        public const string HttpClientName = "tfl";

        public Task<TflResponse> GetAsync(string path, bool appendSubKey);
    }
}
