using System.Linq;
using TubeScan.Telemetry;
using NSubstitute;
using Xunit;

namespace TubeScan.Tests.Telemetry
{
    public class AggregatedTelemetryTests
    {
        [Fact]
        public void Event_EventMessagePropagated()
        {
            var telemetries = Enumerable.Range(1, 3)
                .Select(i => Substitute.For<ITelemetry>())
                .ToArray();

            var aggTelemetry = new AggregatedTelemetry(telemetries);

            var evt = new TelemetryEvent();

            aggTelemetry.Event(evt);

            foreach (var t in telemetries)
            {
                t.Received(1).Event(evt);
            }
        }

        [Fact]
        public void Message_EventMessagePropagated()
        {
            var telemetries = Enumerable.Range(1, 3)
                .Select(i => Substitute.For<ITelemetry>())
                .ToArray();

            var aggTelemetry = new AggregatedTelemetry(telemetries);

            var evt = new TelemetryEvent() { Message = "test" };

            aggTelemetry.Message(evt.Message);

            foreach (var t in telemetries)
            {
                t.Received(1).Event(Arg.Is<TelemetryEvent>(e => e.Message == evt.Message));
            }
        }

        [Fact]
        public void Error_EventMessagePropagated()
        {
            var telemetries = Enumerable.Range(1, 3)
                .Select(i => Substitute.For<ITelemetry>())
                .ToArray();

            var aggTelemetry = new AggregatedTelemetry(telemetries);

            var evt = new TelemetryEvent() { Message = "test" };

            aggTelemetry.Error(evt.Message);

            foreach (var t in telemetries)
            {
                t.Received(1).Event(Arg.Is<TelemetryEvent>(e => e.Message == evt.Message));
            }
        }
    }
}
