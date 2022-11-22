using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class MeshPrimitive
    {
        public required Dictionary<string, IndexedItem> Attributes { get; set; }

        [JsonConverter(typeof(IndexedItemConverter<Accessor>))]
        public Accessor? Indices { get; set; }

        [JsonConverter(typeof(IndexedItemConverter<Material>))]
        public Material? Material { get; set; }
        public ModeEnum? Mode { get; set; }
        public Dictionary<string, int>[]? Targets { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }



        public enum ModeEnum
        { 
            POINTS = 0,
            LINES = 1,
            LINE_LOOP = 2,
            LINE_STRIP = 3,
            TRIANGLES = 4,
            TRIANGLE_STRIP = 5,
            TRIANGLE_FAN = 6,
        }
    }
}
