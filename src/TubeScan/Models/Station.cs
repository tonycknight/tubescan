using Tk.Extensions;

namespace TubeScan.Models
{
    internal class Station
    {
        public Station(string naptanId, string name)
        {
            NaptanId = naptanId;
            Name = name;
            ShortName = name.TrimEnd(" Underground Station");
        }

        public string Name { get; }
        public string ShortName { get; }
        public string NaptanId {  get; }
    }
}
