using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class AnimationChannel
    {
        [JsonConverter(typeof(IndexedItemConverter<AnimationSampler>))]
        public required AnimationSampler? Sampler { get; set; }
        public required AnimationChannelTarget Target { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
