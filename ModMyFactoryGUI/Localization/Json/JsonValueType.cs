using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ModMyFactoryGUI.Localization.Json
{
    [JsonConverter(typeof(StringEnumConverter))]
    enum JsonValueType
    {
        [EnumMember(Value = "string")]
        String,
        [EnumMember(Value = "integer")]
        Integer,
        [EnumMember(Value = "float")]
        Float,
        [EnumMember(Value = "date")]
        Date,
    }
}
