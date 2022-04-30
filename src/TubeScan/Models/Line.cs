namespace TubeScan.Models
{
    internal class Line
    {
        public Line(string id, string name, string colour)
        {
            Id = id;
            Name = name;
            Colour = colour;
        }
        public string Id { get; init; }
        public string Name { get; init; }
        public string Colour { get; init; }
    }
}
