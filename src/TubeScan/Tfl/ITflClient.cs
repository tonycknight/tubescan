namespace TubeScan.Tfl
{
    internal interface ITflClient
    {
        public Task<TflResponse> GetAsync(string path, bool appendSubKey);
    }
}
