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
            var result = domain.CreateUri(route, appKey);

            var u = new Uri(result);

            u.ToString().Should().Be(expectedUri);
        }
    }
}
