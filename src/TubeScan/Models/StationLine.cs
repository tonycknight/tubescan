namespace TubeScan.Models
{
    internal class StationLine
    {
        public StationLine(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; init; }

        public string Name { get; init; }
    }
}
