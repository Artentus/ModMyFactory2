//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ModMyFactory.ModSettings
{
    internal sealed class SettingTypeConverter : JsonConverter<SettingType>
    {
        private static readonly Dictionary<SettingType, string> to =
            new Dictionary<SettingType, string>
            {
                { SettingType.Boolean, "bool-setting" },
                { SettingType.Integer, "int-setting" },
                { SettingType.Double, "double-setting" },
                { SettingType.String, "string-setting" },
            };

        private static readonly Dictionary<string, SettingType> from =
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
