using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class MaterialNormalTextureInfo
    {
        [JsonConverter(typeof(IndexedItemConverter<Texture>))]
        public required Texture Index { get; set; }

        public int? TexCoord { get; set; }
        public float? Scale { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
