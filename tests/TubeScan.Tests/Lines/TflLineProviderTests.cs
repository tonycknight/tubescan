using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TubeScan.Lines;
using TubeScan.Tfl;
using Xunit;

namespace TubeScan.Tests.Lines
{
    public class TflLineProviderTests
    {
        [Fact]
        public async Task GetLineStatusAsync_FailedRequest_ThrowsException()
        {
            var tflResp = new TflResponse()
            {
                IsSuccess = false,
                Body = "[]",
            };
            
            var tfl = Substitute.For<ITflClient>();
            tfl.GetAsync(Arg.Is<string>(s => s.Length > 0), false).Returns(Task.FromResult(tflResp));

            var p = new TflLineStatusProvider(tfl);


            Func<Task<IList<Models.LineStatus>?>> f = async () => await p.GetLineStatusAsync();
            
            await f.Should().ThrowAsync<ApplicationException>().WithMessage("?*");
        }

        [Theory]
        [InlineData("[]", 0)]
        [InlineData("[ {} ]", 1)]
        public async Task GetLineStatusAsync_EmptyJson_ReturnsEmptyList(string json, int expectedCount)
        {
            var tflResp = new TflResponse()
            {
                IsSuccess = true,
                Body = json,
            };

            var tfl = Substitute.For<ITflClient>();
            tfl.GetAsync(Arg.Any<string>(), false).Returns(Task.FromResult(tflResp));

            var p = new TflLineStatusProvider(tfl);

            var result = await p.GetLineStatusAsync();
            result.Should().HaveCount(expectedCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("{}")]
        public async Task GetLineStatusAsync_InvalidJson_ExceptionThrown(string json)
        {
            var tflResp = new TflResponse()
            {
                IsSuccess = true,
                Body = json,
            };

            var tfl = Substitute.For<ITflClient>();
            tfl.GetAsync(Arg.Any<string>(), false).Returns(Task.FromResult(tflResp));

            var p = new TflLineStatusProvider(tfl);

            Func<Task<IList<Models.LineStatus>?>> f = async () => await p.GetLineStatusAsync();

            await f.Should().ThrowAsync<Exception>().WithMessage("?*");
        }

    }
}
