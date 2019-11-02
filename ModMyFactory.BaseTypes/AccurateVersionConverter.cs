using Newtonsoft.Json;
using System;

namespace ModMyFactory.BaseTypes
{
    sealed class AccurateVersionConverter : JsonConverter<AccurateVersion>
    {
        public override AccurateVersion ReadJson(JsonReader reader, Type objectType, AccurateVersion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;
            return AccurateVersion.Parse(value);
        }

        public override void WriteJson(JsonWriter writer, AccurateVersion value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
