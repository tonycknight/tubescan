namespace TubeScan.Tfl
{
    internal class TflResponse
    {
        public bool IsSuccess { get; init; }
        public int HttpStatus { get; init; }
        public string Body { get; init; }
        public IDictionary<string, string[]> Headers { get; init; }
        public Exception Exception { get; init; }
    }
}
