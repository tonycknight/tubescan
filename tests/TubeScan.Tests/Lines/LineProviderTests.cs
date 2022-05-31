using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TubeScan.Lines;
using TubeScan.Models;
using Xunit;
using Tk.Extensions.Tasks;


namespace TubeScan.Tests.Lines
{

    public class LineProviderTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public async Task GetLinesAsync_ReturnsLines(int count)
        {
            var lines = GetLines(count);

            var linesProvider = Substitute.For<ILineReferenceProvider>();
            linesProvider.GetLines().Returns(lines);

            var lineStatusProvider = Substitute.For<ILineStatusProvider>();

            var lp = new LineProvider(linesProvider, lineStatusProvider);

            var result = await lp.GetLinesAsync();

            result.Should().BeEquivalentTo(lines);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public async Task GetLineStatusAsync(int count)
        {
            var lines = GetLines(count);
            var xs = Enumerable.Range(0, count)
                .Select(i => new LineStatus())
                .ToList() as IList<LineStatus>;

            var linesProvider = Substitute.For<ILineReferenceProvider>();
            linesProvider.GetLines().Returns(lines);

            var lineStatusProvider = Substitute.For<ILineStatusProvider>();
            lineStatusProvider.GetLineStatusAsync().Returns(xs.ToTaskResult());

            var result = await lineStatusProvider.GetLineStatusAsync();

            result.Should().BeEquivalentTo(xs);
        }

        private IList<Line> GetLines(int count)
            => Enumerable.Range(0, count)
                .Select(i => new Line(i.ToString(), $"Line {i}", ""))
                .ToList();
    }
}
