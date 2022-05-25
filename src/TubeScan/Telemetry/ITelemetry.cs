namespace TubeScan.Telemetry
{
    internal interface ITelemetry
    {
        void Error(string message);
        void Message(string message);
        void Warning(string message);
        void Debug(string message);
        void Highlight(string message);
        void Event(TelemetryEvent evt);
    }
}
