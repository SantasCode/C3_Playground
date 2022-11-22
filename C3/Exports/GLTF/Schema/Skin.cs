using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class Skin : IndexedItem
    {
        [JsonConverter(typeof(IndexedItemConverter<Accessor>))]
        public Accessor? InverseBindMatrices { get; set; }

        [JsonConverter(typeof(IndexedItemConverter<Node>))]
        public Node? Skeleton { get; set; }

        [JsonConverter(typeof(IndexListConverter<Node>))]
        public required List<Node> Joints { get; set; }

        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
