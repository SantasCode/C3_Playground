using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class AnimationSampler
    {
        [JsonConverter(typeof(IndexedItemConverter<Accessor>))]
        public required Accessor Input { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InterpolationEnum? Interpolation { get; set; }

        [JsonConverter(typeof(IndexedItemConverter<Accessor>))]
        public required Accessor Output { get; set; }

        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }



        public enum InterpolationEnum
        {
            LINEAR,
            STEP,
            CUBICSPLINE,
        }
    }
}
