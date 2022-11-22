using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace C3.Exports.GLTF
{
    internal class IndexListConverter<T> : JsonConverter<List<T>> where T : IndexedItem
    {
        public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            JsonSerializerOptions newOptions = new(options);
            newOptions.WriteIndented = false;

            var indices = value.Select(p => p.Index).ToList();

            writer.WriteRawValue(JsonSerializer.Serialize(indices, newOptions));

        }
    }
}
