using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace C3.Exports.GLTF.Schema
{
    internal class BufferView : IndexedItem
    {
        [JsonConverter(typeof(IndexedItemConverter<Buffer>))]
        public required Buffer Buffer { get; set; }
        public int? ByteOffset { get; set; }
        public required int ByteLength { get; set; }
        public int? ByteStride { get; set; }
        public TargetEnum? Target { get; set; }
        public string? Name { get; set; }

        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }


        public enum TargetEnum
        {
            ARRAY_BUFFER = 34962,
            ELEMENT_ARRAY_BUFFER = 34963,
        }

    }
}
