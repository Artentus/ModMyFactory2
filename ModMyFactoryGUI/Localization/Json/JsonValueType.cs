//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ModMyFactoryGUI.Localization.Json
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum JsonValueType
    {
        [EnumMember(Value = "string")]
        String,

        [EnumMember(Value = "integer")]
        Integer,

        [EnumMember(Value = "float")]
        Float,

        [EnumMember(Value = "date")]
        Date,
    }
}
