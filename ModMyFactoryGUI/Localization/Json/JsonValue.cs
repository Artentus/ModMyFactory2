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
            switch (type)
            {
                case JsonValueType.String:
                    Value = Convert.ToString(value);
                    break;
                case JsonValueType.Integer:
                    Value = Convert.ToInt64(value);
                    break;
                case JsonValueType.Float:
                    Value = Convert.ToDouble(value);
                    break;
                case JsonValueType.Date:
                    Value = Convert.ToDateTime(value);
                    break;
                default:
                    Value = value;
                    break;
            }
        }
    }
}
