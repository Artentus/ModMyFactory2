using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ModMyFactory.ModSettings
{
    sealed class RuntimeTypeConverter : JsonConverter<RuntimeType>
    {
        private static readonly Dictionary<RuntimeType, string> to =
            new Dictionary<RuntimeType, string>
            {
                { RuntimeType.Startup, "startup" },
                { RuntimeType.Global, "runtime-global" },
                { RuntimeType.User, "runtime-per-user" },
            };
        private static readonly Dictionary<string, RuntimeType> from =
            new Dictionary<string, RuntimeType>
            {
                { "startup", RuntimeType.Startup },
                { "runtime-global", RuntimeType.Global },
                { "runtime-per-user", RuntimeType.User },
            };

        public override RuntimeType ReadJson(JsonReader reader, Type objectType, RuntimeType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string value = token.Value<string>();
            return from[value];
        }

        public override void WriteJson(JsonWriter writer, RuntimeType value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(to[value]);
            token.WriteTo(writer);
        }
    }
}
