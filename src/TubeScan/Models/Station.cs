using Tk.Extensions;

namespace TubeScan.Models
{
    internal class Station
    {
        public Station(string naptanId, string name)
            : this(naptanId, name, Enumerable.Empty<StationLine>())
        {

        }

        public Station(string naptanId, string name, IEnumerable<StationLine> lines)
        {
            NaptanId = naptanId;
            Name = name;
            ShortName = name.TrimEnd(" Underground Station");
            Lines = lines.ToList();
        }

        public string Name { get; }
        public string ShortName { get; }
        public string NaptanId {  get; }
        public IList<StationLine> Lines { get; }
    }
}
