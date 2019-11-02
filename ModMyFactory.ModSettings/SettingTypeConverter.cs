﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ModMyFactory.ModSettings
{
    sealed class SettingTypeConverter : JsonConverter<SettingType>
    {
        static readonly Dictionary<SettingType, string> to =
            new Dictionary<SettingType, string>
            {
                { SettingType.Boolean, "bool-setting" },
                { SettingType.Integer, "int-setting" },
                { SettingType.Double, "double-setting" },
                { SettingType.String, "string-setting" },
            };
        static readonly Dictionary<string, SettingType> from =
            new Dictionary<string, SettingType>
            {
                { "bool-setting", SettingType.Boolean },
                { "int-setting", SettingType.Integer },
                { "double-setting", SettingType.Double },
                { "string-setting", SettingType.String },
            };

        public override SettingType ReadJson(JsonReader reader, Type objectType, SettingType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string value = token.Value<string>();
            return from[value];
        }

        public override void WriteJson(JsonWriter writer, SettingType value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(to[value]);
            token.WriteTo(writer);
        }
    }
}
