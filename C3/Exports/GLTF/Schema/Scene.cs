using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class Scene : IndexedItem
    {
        [JsonConverter(typeof(IndexListConverter<Node>))]
        public List<Node>? Nodes { get; set; }
        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
