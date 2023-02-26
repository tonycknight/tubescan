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
            var kind = evt.Kind.ToKindString().FormatKind(evt.Kind).Colourise(evt.Kind);
            kind = kind.Length > 0 ? $"{kind} " : "";

            var line = $"[{evt.Time.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {kind}{evt.Message.Colourise(evt.Kind)}";

            _writeMessage(line);
        }

    }
}
