namespace TubeScan.Telemetry
{
    internal interface ITelemetry
    {        
        void Event(TelemetryEvent evt);
    }
}
