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
    internal static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, AccurateVersion version)
        {
            writer.Write(version.Main);
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Revision);
        }
    }
}
