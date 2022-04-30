namespace TubeScan.Search
{
    internal static class ComparisonExtensions
    {
        public static IEnumerable<SearchInfo<T>> Match<T>(this IEnumerable<T> values,
                                                          string query, Func<T, string> keySelector) 
            => values.Select(v =>   {
                                        var k = keySelector(v);
                                        var s = Score(k, query);

                                        return new SearchInfo<T>(v, s);
                                    })
                     .Where(si => si.Score > 0.0M)
                     .OrderByDescending(si => si.Score);

        private static decimal Score(string value, string comparand)
        {
            if(StringComparer.InvariantCultureIgnoreCase.Equals(value, comparand))
            {
                return decimal.MaxValue;
            }
            if (value.StartsWith(comparand, StringComparison.CurrentCultureIgnoreCase))
            {
                return ((decimal)comparand.Length / value.Length);
            }
            return 0.0M;
        }
    }
}
