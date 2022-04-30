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
            var line = $"[{evt.Time.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {evt.Message}";

            _writeMessage(line);
        }

        public void Message(string message) =>
            Event(new TelemetryEvent { Message = message });

        public void Error(string message) =>
            Event(new TelemetryEvent { Message = message });
    }
}
