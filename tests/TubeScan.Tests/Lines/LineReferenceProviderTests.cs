using System;
using System.Linq;
using FluentAssertions;
using TubeScan.Lines;
using TubeScan.Models;
using Xunit;

namespace TubeScan.Tests.Lines
{

    public class LineReferenceProviderTests
    {
        [Fact]
        public void GetLinesAsync_SetReturned()
        {
            var p = new LineReferenceProvider();

            var r = p.GetLines();

            r.Should().NotBeEmpty();
        }

        [Fact]
        public void GetLinesAsync_ObjectsPopulated()
        {
            var p = new LineReferenceProvider();

            var r = p.GetLines();

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
