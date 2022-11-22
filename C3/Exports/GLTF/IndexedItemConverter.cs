using C3.Exports.GLTF.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace C3.Exports.GLTF
{
    internal class IndexedItemConverter<T> : JsonConverter<T> where T : IndexedItem
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Index);
        }
    }
    internal class IndexedItemConverter : JsonConverter<IndexedItem>
    {
        public override IndexedItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IndexedItem value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Index);
        }
    }
}
