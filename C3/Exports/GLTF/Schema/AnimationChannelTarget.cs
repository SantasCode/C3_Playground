using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class AnimationChannelTarget
    {
        [JsonConverter(typeof(IndexedItemConverter<Node>))]
        public Node? Node { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required PathEnum Path { get; set; }

        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }



        public enum PathEnum
        {
            translation,
            rotation,
            scale,
            weights,
        }
    }
}
