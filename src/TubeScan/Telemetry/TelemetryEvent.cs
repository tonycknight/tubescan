namespace TubeScan.Telemetry
{
    internal enum TelemetryEventKind
    {
        Debug,
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
    }
}
