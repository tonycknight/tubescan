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
            => Event(CreateEvent(TelemetryEventKind.Info, message));

        public void Error(string message)
            => Event(CreateEvent(TelemetryEventKind.Error, message));

        public void Warning(string message)
            => Event(CreateEvent(TelemetryEventKind.Warning, message));

        public void Debug(string message)
            => Event(CreateEvent(TelemetryEventKind.Debug, message));

        public void Highlight(string message)
            => Event(CreateEvent(TelemetryEventKind.Highlight, message));

        private TelemetryEvent CreateEvent(TelemetryEventKind kind, string message)
            => new TelemetryEvent() { Kind = kind, Message = message };
    }
}
