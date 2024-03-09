using System;
using FluentAssertions;
using TubeScan.Tfl;
using Xunit;

namespace TubeScan.Tests.Tfl
{
    public class TflExtensionsTests
    {
        [Theory]
        [InlineData("http://test.com", "", null, "http://test.com/")]
        [InlineData("http://test.com", "", "abc", "http://test.com/?appKey=abc")]
        [InlineData("http://test.com", "test", null, "http://test.com/test")]
        [InlineData("http://test.com", "test", "abc", "http://test.com/test?appKey=abc")]
        [InlineData("http://test.com", "?test", null, "http://test.com/?test")]
        [InlineData("http://test.com", "?test", "abc", "http://test.com/?test&appKey=abc")]
        [InlineData("http://test.com", "test?xyz=true", null, "http://test.com/test?xyz=true")]
        [InlineData("http://test.com", "test?xyz=true", "abc", "http://test.com/test?xyz=true&appKey=abc")]
        public void CreateUri_UriComposed(string domain, string route, string? appKey, string expectedUri)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            var result = domain.CreateUri(route, appKey);
#pragma warning restore CS8604 // Possible null reference argument.

            var u = new Uri(result);

            u.ToString().Should().Be(expectedUri);
        }
    }
}
