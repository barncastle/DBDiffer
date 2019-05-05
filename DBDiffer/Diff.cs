using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DBDiffer
{
    public class Diff
    {
        public Diff(DiffOperation operation, string path, object curvalue = null, object prevvalue = null)
        {
            Operation = operation;
            Property = path ?? "";
            CurrentValue = curvalue;
            PreviousValue = prevvalue;
        }

        [JsonProperty("op")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DiffOperation Operation { get; set; }

        [JsonProperty("property")]
        public string Property { get; private set; }

        [JsonProperty("currentvalue")]
        public object CurrentValue { get; set; }

        [JsonProperty("previousvalue")]
        public object PreviousValue { get; set; }

        public override string ToString() => $"{Operation} {Property} {CurrentValue} {PreviousValue}";
    }
}
