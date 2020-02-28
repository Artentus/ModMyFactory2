using Newtonsoft.Json;
using System;

namespace ModMyFactoryGUI.Localization.Json
{
    sealed class JsonValue
    {
        [JsonProperty("type")]
        public JsonValueType Type { get; }

        [JsonProperty("value")]
        public object Value { get; }

        [JsonConstructor]
        public JsonValue(JsonValueType type, object value)
        {
            Type = type;
            Value = type switch
            {
                JsonValueType.String => Convert.ToString(value),
                JsonValueType.Integer => Convert.ToInt64(value),
                JsonValueType.Float => Convert.ToDouble(value),
                JsonValueType.Date => Convert.ToDateTime(value),
                _ => value,
            };
        }
    }
}
