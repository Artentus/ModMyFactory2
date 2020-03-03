//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

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
