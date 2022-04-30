namespace TubeScan.Models
{
    internal class LineStatus
    {
        public string Id { get; set; }

        public IList<LineHealthStatus> HealthStatuses { get; set; }
    }

    internal class LineHealthStatus
    {
        public string TflHealth { get; set; }
        public HealthStatus Health { get; set; }
        public string Description { get; set; }
        public IList<Station> AffectedStations { get; set; }
    }
}
