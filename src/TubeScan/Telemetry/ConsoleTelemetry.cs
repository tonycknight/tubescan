using Crayon;

namespace TubeScan.Telemetry
{
    internal class ConsoleTelemetry : ITelemetry
    {
        private readonly Action<string> _writeMessage;

        public ConsoleTelemetry() : this(Console.WriteLine)
        {
        }

        public ConsoleTelemetry(Action<string> writeMessage)
        {
            _writeMessage = writeMessage;
        }

        public void Event(TelemetryEvent evt)
        {
            var kind = Kind(evt.Kind);
            kind = kind.Length > 0 ? $"{kind} " : "";

            var line = $"[{evt.Time.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {kind}{Colour(evt.Kind, evt.Message)}";

            _writeMessage(line);
        }

        private string Kind(TelemetryEventKind kind) {
            var msg = kind switch
            {
                TelemetryEventKind.Error => "ERROR",
                TelemetryEventKind.Trace => "TRACE",
                TelemetryEventKind.Info => "INFO",
                TelemetryEventKind.Warning => "WARN",
                _ => ""
            };

            return msg.Length > 0 ? $"[{Colour(kind, msg)}]" : msg;
        }

        private string Colour(TelemetryEventKind kind, string message)
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
