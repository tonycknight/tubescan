namespace TubeScan.Models
{
    internal class Arrival
    {
        public string LineId { get; set; }
        public string VehicleId { get; set; }
        public string DestinationId { get; set; }
        public string CurrentLocation { get; set; }
        public string ArrivalPlatform { get; set; }
        public DateTimeOffset ExpectedArrival { get; set; }
    }
}
