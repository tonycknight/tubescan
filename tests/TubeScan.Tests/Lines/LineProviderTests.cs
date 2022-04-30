using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using TubeScan.Lines;
using TubeScan.Models;
using Xunit;

namespace TubeScan.Tests.Lines
{

    public class LineProviderTests
    {
        internal class StubLineProvider : LineProvider
        {
            public override Task<IList<LineStatus>> GetLineStatusAsync() => throw new NotImplementedException();
        }

        [Fact]
        public async Task GetLinesAsync_SetReturned()
        {
            var p = new StubLineProvider();

            var r = await p.GetLinesAsync();

            r.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetLinesAsync_ObjectsPopulated()
        {
            var p = new StubLineProvider();

            var r = await p.GetLinesAsync();

            Func<Line, bool> isPopulated = s =>
            {
                s.Id.Should().NotBeNullOrEmpty();
                s.Name.Should().NotBeNullOrEmpty();
                s.Colour.Should().NotBeNullOrEmpty();
                return true;
            };

            r.All(isPopulated).Should().BeTrue();
        }
    }
}
