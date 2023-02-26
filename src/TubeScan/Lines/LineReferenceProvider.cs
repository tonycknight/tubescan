using TubeScan.Models;

namespace TubeScan.Lines
{
    internal class LineReferenceProvider : ILineReferenceProvider
    {
        private readonly IList<Line> _lines = CreateLines();

        public IList<Line> GetLines() => _lines;

        private static IList<Line> CreateLines()
            => new Line[]
                {
                    new Line("piccadilly", "Piccadilly", "#0019A8"),
                    new Line("bakerloo", "Bakerloo", "#B26313"),
                    new Line("central", "Central", "#DC241F"),
                    new Line("circle", "Circle", "#FFD329"),
                    new Line("district", "District", "#007D32"),
                    new Line("hammersmith-city", "Hammersmith & City", "#F4A9BE"),
                    new Line("jubilee", "Jubilee", "#A1A5A7"),
                    new Line("metropolitan", "Metropolitan", "#9B0058"),
                    new Line("northern", "Northern", "#000000"),
                    new Line("victoria", "Victoria", "#0098D8"),
                    new Line("waterloo-city", "Waterloo & City", "#93CEBA")
                };
    }
}
