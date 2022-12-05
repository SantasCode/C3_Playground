using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class Gltf
    {
        public List<string>? ExtensionsUsed { get; set; }
        public List<string>? ExtensionsRequired { get; set; }
        public IndexedList<Accessor>? Accessors { get; set; }
        public IndexedList<Animation>? Animations { get; set; }
        public required Asset Asset { get; set; }
        public IndexedList<Buffer>? Buffers { get; set; }
        public IndexedList<BufferView>? BufferViews { get; set; }
        public IndexedList<Camera>? Cameras { get; set; }
        public IndexedList<Image>? Images { get; set; }
        public IndexedList<Material>? Materials { get; set; }
        public IndexedList<Mesh>? Meshes { get; set; }
        public IndexedList<Node>? Nodes { get; set; }
        public IndexedList<Sampler>? Samplers { get; set; }
        
        [JsonConverter(typeof(IndexedItemConverter<Scene>))]
        public Scene? Scene { get; set; }

        public IndexedList<Scene>? Scenes { get; set; }
        public IndexedList<Skin>? Skins { get; set; }
        public IndexedList<Texture>? Textures { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }

    }
}