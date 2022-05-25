namespace TubeScan.Telemetry
{
    internal static class TelemetryExtensions
    {
        public static void Send(this TelemetryEvent evt, ITelemetry telemetry) => telemetry.Event(evt);

        public static TelemetryEvent CreateTelemetryEvent(this string message)
            => TelemetryEvent.Create(message);

        public static TelemetryEvent CreateTelemetryEvent(this string message, TelemetryEventKind kind)
            => TelemetryEvent.Create(kind, message);
    }
}
