//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace ModMyFactory.ModSettings.Serialization
{
    internal static class JTokenTypeExtensions
    {
        public static PropertyTreeType ToTreeType(this JTokenType type)
        {
            switch (type)
            {
                case JTokenType.Object: return PropertyTreeType.Dictionary;
                case JTokenType.Array: return PropertyTreeType.List;
                case JTokenType.Integer: return PropertyTreeType.Number;
                case JTokenType.Float: return PropertyTreeType.Number;
                case JTokenType.String: return PropertyTreeType.String;
                case JTokenType.Boolean: return PropertyTreeType.Bool;
                default: throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(JTokenType));
            }
        }
    }
}
