using System;
using TubeScan.Models;
using TubeScan.Lines;
using Xunit;
using FluentAssertions;

namespace TubeScan.Tests.Lines
{
    public class LineStatusExtensionsTests
    {
        [Fact]
        public void GetDeltas_VanillaObjects()
        {
            var existingStatus = new[] { new LineStatus() };
            var newStatus = new[] { new LineStatus() };
                              
            var deltas = existingStatus.GetDeltas(newStatus);

            deltas.Should().BeEmpty();
        }

        [Theory]
        [InlineData("a", "b", "ok", "ok")]
        [InlineData("a", "b", "bad service", "bad service")]
        
        public void GetDeltas_OldExists_NoDeltas(string oldId, string newId, string oldStatus, string newStatus)
        {
            var existingLineStatus = new[] { new LineStatus() { Id = oldId, HealthStatuses = new[] { new LineHealthStatus() { TflHealth = oldStatus } } } };
            var newLineStatus = new[] { new LineStatus() { Id = newId, HealthStatuses = new[] { new LineHealthStatus() { TflHealth = newStatus } } } };

            var deltas = existingLineStatus.GetDeltas(newLineStatus);

            deltas.Should().BeEmpty();            
        }


        [Theory]
        [InlineData("ok", "ok", false)]
        [InlineData("ok", "OK", false)]
        [InlineData("bad service", "bad service", false)]
        [InlineData("bad service", "Bad service", false)]
        [InlineData("ok", "bad service", true)]
        [InlineData("bad service", "ok", true)]
        public void GetDeltas_SingletonValues_StatusDiffer(string oldStatus, string newStatus, bool differenceExpected)
        {
            var existingLineStatus = new[] { new LineStatus() { HealthStatuses = new[] { new LineHealthStatus() { TflHealth = oldStatus, Description = Guid.NewGuid().ToString() } } } };
            var newLineStatus = new[] { new LineStatus() { HealthStatuses = new[] { new LineHealthStatus() { TflHealth = newStatus, Description = Guid.NewGuid().ToString() } } } };

            var deltas = existingLineStatus.GetDeltas(newLineStatus);


            if (!differenceExpected)
            {
                deltas.Should().BeEmpty();
            }
            else
            {
                var expected = newLineStatus;
                deltas.Should().BeEquivalentTo(expected);
            }
        }


        [Theory]
        [InlineData("ok", "ok", false)]
        [InlineData("ok", "OK", false)]
        [InlineData("bad service", "bad service", false)]
        [InlineData("bad service", "Bad Service", false)]
        [InlineData("ok", "bad service", true)]
        [InlineData("bad service", "ok", true)]
        public void GetDeltas_MultipleValues_StatusDiffer(string oldStatus, string newStatus, bool differenceExpected)
        {
            var existingLineStatus = new[] { new LineStatus()
                                                        { HealthStatuses = new[] {
                                                            new LineHealthStatus() { TflHealth = "ok", Description = Guid.NewGuid().ToString() },
                                                            new LineHealthStatus() { TflHealth = oldStatus, Description = Guid.NewGuid().ToString() },
                                                            new LineHealthStatus() { TflHealth = "ok", Description = Guid.NewGuid().ToString() }
                                                        }}};

            var newLineStatus = new[] { new LineStatus()
                                                        { HealthStatuses = new[] {
                                                            new LineHealthStatus() { TflHealth = "ok", Description = Guid.NewGuid().ToString() },
                                                            new LineHealthStatus() { TflHealth = "ok", Description = Guid.NewGuid().ToString() },
                                                            new LineHealthStatus() { TflHealth = newStatus, Description = Guid.NewGuid().ToString() }
                                                        }}};

            var deltas = existingLineStatus.GetDeltas(newLineStatus);

            if (!differenceExpected)
            {
                deltas.Should().BeEmpty();
            }
            else
            {
                var expected = newLineStatus;
                deltas.Should().BeEquivalentTo(expected);
            }
        }
    }
}
