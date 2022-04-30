namespace TubeScan.Search
{
    internal struct SearchInfo<T>
    {
        public SearchInfo(T value, decimal score)
        {
            Value = value;
            Score = score;
        }

        public T Value { get; }
        public decimal Score { get; }
    }
}
