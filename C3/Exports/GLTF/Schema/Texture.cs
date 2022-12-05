using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class Texture : IndexedItem
    {
        [JsonConverter(typeof(IndexedItemConverter<Sampler>))]
        public Sampler? Sampler { get; set; }

        [JsonConverter(typeof(IndexedItemConverter<Image>))]
        public Image? Source { get; set; }

        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
