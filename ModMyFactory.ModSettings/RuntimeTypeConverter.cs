//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.ModSettings.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ModMyFactory.ModSettings
{
    internal sealed class RuntimeTypeConverter : JsonConverter<RuntimeType>
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
            string? value = token.Value<string>();
            try
            {
                return from[value!];
            }
            catch (KeyNotFoundException ex)
            {
                throw new SerializerException("Unknown runtime type", ex);
            }
            catch (ArgumentNullException ex)
            {
                throw new SerializerException("Runtime type undefined", ex);
            }
        }

        public override void WriteJson(JsonWriter writer, RuntimeType value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(to[value]);
            token.WriteTo(writer);
        }
    }
}
