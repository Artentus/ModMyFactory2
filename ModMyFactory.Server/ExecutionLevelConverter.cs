//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;

namespace ModMyFactory.Server
{
    internal sealed class ExecutionLevelConverter : JsonConverter<ExecutionLevel>
    {
        public override ExecutionLevel ReadJson(JsonReader reader, Type objectType, ExecutionLevel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var enumString = reader.Value as string;

            return enumString switch
            {
                "true" => ExecutionLevel.Everyone,
                "admins-only" => ExecutionLevel.AdminsOnly,
                "false" => ExecutionLevel.Noone,
                _ => ExecutionLevel.AdminsOnly
            };
        }

        public override void WriteJson(JsonWriter writer, ExecutionLevel value, JsonSerializer serializer)
        {
            switch (value)
            {
                case ExecutionLevel.Everyone:
                    writer.WriteValue("true");
                    break;

                case ExecutionLevel.AdminsOnly:
                    writer.WriteValue("admins-only");
                    break;

                case ExecutionLevel.Noone:
                    writer.WriteValue("false");
                    break;

                default:
                    writer.WriteValue("admins-only");
                    break;
            }
        }
    }
}
