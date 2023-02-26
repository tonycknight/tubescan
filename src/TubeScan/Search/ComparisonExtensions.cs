using System.Text;
using Tk.Extensions;
using Tk.Extensions.Linq;

namespace TubeScan.Search
{
    internal static class ComparisonExtensions
    {
        private static readonly char[] WordDelim = " .,!?[]<>()-".ToCharArray();

        public static IEnumerable<SearchInfo<T>> Match<T>(this IEnumerable<T> values,
                                                          string query, Func<T, string> keySelector)
        {
            var results = values.MatchInner(query, keySelector)
                .Where(si => si.Score < int.MaxValue)
                .OrderBy(si => si.Score)
                .Take(5).ToList();

            var best = results.FirstOrDefault().Score;
            var topResults = results.Where(si => si.Score == best);

            Func<string, int> getScore = key =>
            {
                var keyScores = key.Tokenise()
                                   .Select(k =>
                                   {
                                       var score = k.StripPunctuation().GetDamerauLevenshteinDistance(query, true);
                                       return score == 0 ? -1 : score;
                                   });

                return keyScores.Sum();
            };

            return topResults.Select(si => new { si = si, score = getScore(keySelector(si.Value)) })
                             .OrderBy(a => a.score)
                             .Select(a => a.si);
        }

        private static IEnumerable<SearchInfo<T>> MatchInner<T>(this IEnumerable<T> values,
                                                          string query, Func<T, string> keySelector)
        {
            var qs = query.Tokenise().Select(StripPunctuation).ToList();

            foreach (var value in values)
            {
                var vs = keySelector(value).Tokenise().Select(StripPunctuation).Where(s => s.Length > 0);

                var scores = vs.Zip(qs.ToInfinite(), (v, q) => (v, q))
                               .Select(p => new { p = p, s = p.v.GetDamerauLevenshteinDistance(p.q, true) });

                var hits = scores.Where(a => a.s <= 2)
                               .Select(a => a.s)
                               .ToList();

                var score = hits.Any()
                    ? hits.Take(3).Sum()
                    : int.MaxValue;

                yield return new SearchInfo<T>(value, score);
            }
        }

        public static IEnumerable<string> Tokenise(this string value)
            => value.TokeniseInner().Where(s => s?.Length > 0);


        public static string StripPunctuation(this string value)
        {
            var result = new StringBuilder();

            foreach (var c in value)
            {
                if (!char.IsPunctuation(c))
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }


        private static IEnumerable<string> TokeniseInner(this string value)
        {
            var i = 0;
            string s;

            for (var j = 0; j < value.Length; j++)
            {
                var c = value[j];
                if (WordDelim.Contains(c))
                {
                    s = value.Substring(i, j - i).Trim();
                    i = j;
                    yield return s;
                }
            }

            s = value.Substring(i, value.Length - i).Trim();
            yield return s;
        }
    }
}
