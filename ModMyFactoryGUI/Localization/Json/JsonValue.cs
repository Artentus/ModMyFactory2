//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;

namespace ModMyFactoryGUI.Localization.Json
{
    internal sealed class JsonValue
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
