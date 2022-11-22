using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class Material : IndexedItem
    {
        public string? Name { get; set; }
        public MaterialPbrMetallicRoughness? PbrMetallicRoughness { get; set; }
        public MaterialNormalTextureInfo? NormalTexture { get; set; }
        public MaterialOcclusionTextureInfo? OcclusionTexture { get; set; }
        public TextureInfo? EmissiveTexture { get; set; }
        public float[]? EmissiveFactor { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlphaModeEnum? AlphaMode { get; set; }
        public float? AlphaCutoff { get; set; }
        public bool? DoubleSided { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }


        public enum AlphaModeEnum
        { 
            OPAQUE,
            MASK,
            BLEND,
        }
    }
}
