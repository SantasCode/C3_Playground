using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Exports.GLTF.Schema
{
    internal class Asset
    {
        public string? Copyright { get; set; }
        public string? Generator { get; set; }
        public required string Version { get; set; }
        public string? MinVersion { get; set; }

        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
