using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Exports.GLTF.Schema
{
    internal class Buffer : IndexedItem
    {
        public string? Uri { get; set; }
        public required int ByteLength { get; set; }
        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
