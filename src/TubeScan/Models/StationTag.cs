using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeScan.Models
{
    internal class StationTag
    {
        public StationTag(string naptanId, string tag)
        {
            NaptanId = naptanId;
            Tag = tag;
        }

        public string NaptanId { get; init; }
        public string Tag {  get; init; }
    }
}
