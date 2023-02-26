using System;
using System.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using TubeScan.Search;
using Xunit;

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
                                    .Select(si => si.Value)
                                    .FirstOrDefault();

                return StringComparer.InvariantCultureIgnoreCase.Equals(result, expected);
            };

            return rule.When(count.Get >= 2);
        }

        [Property(Verbose = true)]
        public Property Match_NoMatch_NotFound(NonEmptyString[] values, Guid query)
        {
            Func<bool> rule = () =>
            {
                var values2 = values.Select(x => x.Get + x.Get).ToList();

                var result = values2.Match(query.ToString(), x => x).Select(si => si.Value).FirstOrDefault();

                return result == default;
            };

            return rule.When(values.Length > 0);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData("a!A", "aA")]
        [InlineData("King's", "Kings")]
        [InlineData(" King's ", " Kings ")]
        public void StripPunctuation_CharsMapped(string value, string expected)
        {
            var r = value.StripPunctuation();

            r.Should().Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a", "a")]
        [InlineData("a b", "a", "b")]
        [InlineData("aA bb CC", "aA", "bb", "CC")]
        [InlineData(" aA bb CC ", "aA", "bb", "CC")]
        [InlineData("a b c! ", "a", "b", "c", "!")]
        [InlineData("King's Cross", "King's", "Cross")]
        public void Tokenise_ValuesSliced(string value, params string[] expected)
        {
            var r = value.Tokenise().ToList();
            r.Should().BeEquivalentTo(expected);
        }
    }
}
