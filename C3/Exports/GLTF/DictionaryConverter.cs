using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace C3.Exports.GLTF
{
    internal class DictionaryConverter<TKey, TValue, TValueConverter> : JsonConverter<Dictionary<TKey, TValue>> 
        where TValueConverter : JsonConverter<TValue> , new()
        where TKey : notnull
    {
        private readonly static JsonConverter<TKey> keyDefaultConverter =
            (JsonConverter< TKey>)JsonSerializerOptions.Default.GetConverter(typeof(TKey));

        public override Dictionary<TKey, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            var valueConverter = new TValueConverter();

            foreach(KeyValuePair<TKey, TValue> kvp in value)
            {
                writer.WritePropertyName(JsonEncodedText.Encode(kvp.Key.ToString(), encoder: null));

                valueConverter.Write(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}
