namespace TubeScan.Models
{
    internal class StationStatus
    {
        public StationStatus(string naptanId)
        {
            NaptanId = naptanId;            
        }

        public string NaptanId { get; init; }
        
        public StationCrowding Crowding { get; set; }

        public IList<Arrival> Arrivals { get; set; }
    }
}
