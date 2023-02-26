using Crayon;

namespace TubeScan.Telemetry
{
    internal static class TelemetryExtensions
    {
        public static void Send(this TelemetryEvent evt, ITelemetry telemetry) => telemetry.Event(evt);

        public static TelemetryEvent CreateTelemetryEvent(this string message)
            => TelemetryEvent.Create(TelemetryEventKind.Info, message);

        public static TelemetryEvent CreateTelemetryEvent(this string message, TelemetryEventKind kind)
            => TelemetryEvent.Create(kind, message);

        public static string ToKindString(this TelemetryEventKind kind)
            => kind switch
            {
                TelemetryEventKind.Error => "ERROR",
                TelemetryEventKind.Trace => "TRACE",
                TelemetryEventKind.Info => "INFO",
                TelemetryEventKind.Warning => "WARN",
                _ => ""
            };

        public static string FormatKind(this string msg, TelemetryEventKind kind)
            => msg.Length > 0 ? $"[{msg.Colourise(kind)}]" : msg;

        public static string Colourise(this string message, TelemetryEventKind kind)
            => kind switch
            {
                TelemetryEventKind.Error => Output.Bright.Red(message),
                TelemetryEventKind.Trace => Output.Dim(message),
                TelemetryEventKind.Info => Output.Bright.White(message),
                TelemetryEventKind.Warning => Output.Bright.Yellow(message),
                TelemetryEventKind.Highlight => Output.Bright.Cyan(message),
                _ => message
            };
    }
}
