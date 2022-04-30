using System;
using System.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using TubeScan.Search;

namespace TubeScan.Tests.Search
{
    public class ComparisonExtensionsTests
    {
        [Property(Verbose = true)]
        public Property Match_Match_Found(PositiveInt count)
        {
            Func<bool> rule = () =>
            {
                var values = Enumerable.Range(1, count.Get)
                                       .Select(x => "B" + new String('A', x))
                                       .ToList();

                var query = values.Last();

                var result = values.Match(query, x => x)
                                     .Where(si => si.Score == decimal.MaxValue)
                                     .Select(si => si.Value)
                                     .FirstOrDefault();

                return StringComparer.InvariantCultureIgnoreCase.Equals(result, query);
            };

            return rule.When(count.Get > 1);
        }

        [Property(Verbose = true)]
        public Property Match_PartialMatch_Found(PositiveInt count)
        {
            Func<bool> rule = () =>
            {
                var values = Enumerable.Range(1, count.Get)
                                       .Select(x => "B" + new String('A', x))
                                       .ToList();
                
                var expected = values.First();

                var result = values.Match("B", x => x)
                                    .Where(si => si.Score >= 0.5M && si.Score < 1.0M)
                                    .Select(si => si.Value)
                                    .FirstOrDefault();

                return StringComparer.InvariantCultureIgnoreCase.Equals(result, expected);
            };

            return rule.When(count.Get >= 2);
        }

        [Property(Verbose = true)]
        public Property Match_PartialMatch_FoundInOrder(PositiveInt count)
        {
            Func<bool> rule = () =>
            {
                var values = Enumerable.Range(1, count.Get)
                                       .Select(x => "B" + new String('A', x))
                                       .ToList();

                var expected = values.Last();

                var result = values.Match("B", x => x)
                                    .Select(si => si.Value)
                                    .LastOrDefault();

                return StringComparer.InvariantCultureIgnoreCase.Equals(result, expected);
            };

            return rule.When(count.Get >= 2);
        }


        [Property(Verbose = true)]
        public Property Match_NoMatch_NotFound(NonEmptyString[] values)
        {
            Func<bool> rule = () =>
            {
                var values2 = values.Select(x => x.Get + x.Get).ToList();

                var query = Guid.NewGuid().ToString();

                var result = values2.Match(query, x => x).Select(si => si.Value).FirstOrDefault();

                return result == default;
            };

            return rule.When(values.Length > 0);
        }
    }
}
