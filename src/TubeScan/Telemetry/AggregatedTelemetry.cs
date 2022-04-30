namespace TubeScan.Telemetry
{
    internal class AggregatedTelemetry : ITelemetry
    {
        private readonly IList<ITelemetry> _telemetries;

        public AggregatedTelemetry(IList<ITelemetry> telemetries)
        {
            _telemetries = telemetries;
        }

        public void Event(TelemetryEvent evt)
        {
            foreach (var t in _telemetries)
            {
                t.Event(evt);
            }
        }

        public void Message(string message)
        {
            var evt = new TelemetryEvent() { Message = message };
            Event(evt);
        }

        public void Error(string message)
        {
            var evt = new TelemetryEvent() { Message = message };
            Event(evt);
        }
    }
}
