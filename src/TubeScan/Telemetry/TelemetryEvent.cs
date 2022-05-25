namespace TubeScan.Telemetry
{
    internal enum TelemetryEventKind
    {
        Trace,
        Info,
        Warning,
        Error,
        Highlight
    }

    internal record TelemetryEvent
    {
        public DateTime Time { get; init; } = DateTime.UtcNow;
        public TelemetryEventKind Kind { get; init; } = TelemetryEventKind.Info;
        public string Message { get; init; } = default!;

        public static TelemetryEvent Create(string message)
            => Create(TelemetryEventKind.Info, message);

        public static TelemetryEvent Create(TelemetryEventKind kind, string message)
            => new TelemetryEvent()
            {
                Kind = kind,
                Message = message,
            };
    }
}
