using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Exports.GLTF.Schema
{
    internal class Animation : IndexedItem
    {
        public required List<AnimationChannel> Channels { get; set; }
        public required List<AnimationSampler> Samplers { get; set; }
        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
