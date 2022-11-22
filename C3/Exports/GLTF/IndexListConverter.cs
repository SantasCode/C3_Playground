using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;

namespace C3.Exports.GLTF
{
    internal class IndexListConverter<T> : JsonConverter<List<T>> where T : IndexedItem
    {
        private readonly static JsonConverter<List<int>> intArrayDefaultConverter =
            (JsonConverter<List<int>>)JsonSerializerOptions.Default.GetConverter(typeof(List<int>));
        public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            var indices = value.Select(p => p.Index).ToList();

            intArrayDefaultConverter.Write(writer, indices, options);
        }
    }
}
