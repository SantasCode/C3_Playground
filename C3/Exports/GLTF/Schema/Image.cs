using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace C3.Exports.GLTF.Schema
{
    internal class Image : IndexedItem
    {
        public string? Uri { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MimeTypeEnum? MimeType { get; set; }

        [JsonConverter(typeof(IndexedItemConverter<BufferView>))]
        public BufferView? BufferView { get; set; }
        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }


        public enum MimeTypeEnum
        { 
            [EnumMember(Value = "image/jpeg")]
            image_jpeg,

            [EnumMember(Value = "image/png")]
            image_png,
        }
    }
}
