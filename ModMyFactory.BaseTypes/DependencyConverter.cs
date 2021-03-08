//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;

namespace ModMyFactory.BaseTypes
{
    internal sealed class DependencyConverter : JsonConverter<Dependency>
    {
        public override Dependency ReadJson(JsonReader reader, Type objectType, Dependency? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string? value = reader.Value as string;
            return Dependency.Parse(value);
        }

        public override void WriteJson(JsonWriter writer, Dependency? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString() ?? "null");
        }
    }
}
