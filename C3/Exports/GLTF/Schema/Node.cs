using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class Node : IndexedItem 
    {
        [JsonConverter(typeof(IndexedItemConverter<Camera>))]
        public Camera? Camera { get; set; }

        //TODO:Verify this serialization
        [JsonConverter(typeof(IndexListConverter<Node>))]
        public List<Node>? Children { get; set; }

        [JsonConverter(typeof(IndexedItemConverter<Skin>))]
        public Skin? Skin { get; set; }

        public float[]? Matrix { get; set; }

        [JsonConverter(typeof(IndexedItemConverter<Mesh>))]
        public Mesh? Mesh { get; set; }

        public float[]? Rotation { get; set; }
        public float[]? Scale { get; set; }
        public float[]? Translation { get; set; }
        public float[]? Weights { get; set; }
        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }

    }
}
