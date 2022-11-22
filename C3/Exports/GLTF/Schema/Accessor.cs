using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class Accessor : IndexedItem
    {
        [JsonConverter(typeof(IndexedItemConverter<BufferView>))]
        public BufferView? BufferView { get; set; }
        public int? ByteOffset { get; set; }
        
        [JsonRequired]
        public required ComponentTypeEnum ComponentType { get; set; }

        public bool? Normalized { get; set; }
        public required int Count { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required TypeEnum Type { get; set; }
        public float[]? Max { get; set; }
        public float[]? Min { get; set; }
        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }


        public enum ComponentTypeEnum
        {

            BYTE = 5120,

            UNSIGNED_BYTE = 5121,

            SHORT = 5122,

            UNSIGNED_SHORT = 5123,

            UNSIGNED_INT = 5125,

            FLOAT = 5126,
        }

        public enum TypeEnum
        {

            SCALAR,

            VEC2,

            VEC3,

            VEC4,

            MAT2,

            MAT3,

            MAT4,
        }
    }
}
