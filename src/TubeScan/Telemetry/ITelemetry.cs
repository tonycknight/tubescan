namespace TubeScan.Telemetry
{
    internal interface ITelemetry
    {
        void Error(string message);
        void Message(string message);
        void Event(TelemetryEvent evt);
    }
}
