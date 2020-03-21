//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using Newtonsoft.Json;
using System;

namespace ModMyFactory.WebApi.Mods
{
    internal sealed class SHA1HashConverter : JsonConverter<SHA1Hash>
    {
        public override SHA1Hash ReadJson(JsonReader reader, Type objectType, SHA1Hash existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return SHA1Hash.Parse(s);
        }

        public override void WriteJson(JsonWriter writer, SHA1Hash value, JsonSerializer serializer)
        {
            string s = value.ToString();
            writer.WriteValue(s);
        }
    }
}
