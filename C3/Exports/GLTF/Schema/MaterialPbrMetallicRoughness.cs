namespace C3.Exports.GLTF.Schema
{
    internal class MaterialPbrMetallicRoughness 
    {
        public float[]? BaseColorFactor { get; set; }
        public TextureInfo? BaseColorTexture { get; set; }
        public float? MetallicFactor { get; set; }
        public float? RoughnessFactor { get; set; }
        public TextureInfo? MetallicRoughnessTexture { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
