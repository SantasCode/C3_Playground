namespace C3.Exports.GLTF.Schema
{
    internal class Mesh : IndexedItem
    {
        public required List<MeshPrimitive> Primitives { get; set; }
        public float[]? Weights { get; set; }
        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
