namespace TubeScan.Models
{
    internal enum HealthStatus
    {
        Unknown,
        GoodService,
        MinorDelays,
        PartialService,
        SevereDelays,
        NoService
    }
}
