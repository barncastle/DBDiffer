using System;
using DBDiffer.DiffResults;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBDiffer.Helpers.Converters
{
    internal class WoWToolsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(WoWToolsDiff);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var diff = value as WoWToolsDiff;
            var obj = JObject.FromObject(diff.Record);

            // open the entry
            writer.WriteStartObject();

            // write the db record
            writer.WritePropertyName("row");
            writer.WriteStartObject();
            foreach (var x in obj)
            {
                if (x.Value.Type == JTokenType.Array)
                {
                    var array = x.Value as JArray;
                    for (int i = 0; i < array.Count; i++)
                    {
                        writer.WritePropertyName($"{x.Key}[{i}]");
                        writer.WriteValue(array[i]);
                    }
                }
                else
                {
                    writer.WritePropertyName(x.Key);
                    writer.WriteValue(x.Value);
                }
            }
            writer.WriteEndObject();

            // write Operation as it's string representation
            writer.WritePropertyName("op");
            writer.WriteValue(diff.Operation.ToString());
            // use default serialization for the diffs
            writer.WritePropertyName("diff");
            serializer.Serialize(writer, diff.Diffs);

            // close the entry
            writer.WriteEndObject();
        }
    }
}
