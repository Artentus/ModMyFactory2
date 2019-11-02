using Newtonsoft.Json;
using System;

namespace ModMyFactory.BaseTypes
{
    sealed class DependencyConverter : JsonConverter<Dependency>
    {
        public override Dependency ReadJson(JsonReader reader, Type objectType, Dependency existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;
            return Dependency.Parse(value);
        }

        public override void WriteJson(JsonWriter writer, Dependency value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
