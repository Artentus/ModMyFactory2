//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using System.IO;

namespace ModMyFactory.ModSettings.Serialization
{
    internal static class BinaryReaderExtensions
    {
        public static AccurateVersion ReadVersion(this BinaryReader reader)
        {
            var main = reader.ReadUInt16();
            var major = reader.ReadUInt16();
            var minor = reader.ReadUInt16();
            var revisio = reader.ReadUInt16();
            return new AccurateVersion(main, major, minor, revisio);
        }
    }
}
