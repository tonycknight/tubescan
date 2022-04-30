﻿using TubeScan.Models;

namespace TubeScan.Lines
{
    internal abstract class LineProvider : ILineProvider
    {
        private IList<Line> _lines;

        public LineProvider()
        {
            _lines = new Line[]
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

        public Task<IList<Line>> GetLinesAsync() => Task.FromResult(_lines);

        public abstract Task<IList<LineStatus>> GetLineStatusAsync();
    }
}
