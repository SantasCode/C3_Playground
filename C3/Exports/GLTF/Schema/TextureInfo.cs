using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class TextureInfo
    {
        [JsonConverter(typeof(IndexedItemConverter<Texture>))]
        public Texture? Index { get; set; }
        public int TexCoord { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
