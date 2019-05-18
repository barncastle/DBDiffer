using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DBDiffer.Helpers.Converters
{
    internal class ByteArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(byte[]);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is byte[] data)
                writer.WriteValue("[" + string.Join(", ", data) + "]");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
