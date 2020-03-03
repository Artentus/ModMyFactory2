//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Diagnostics;
using System.Reflection;

namespace ModMyFactoryGUI.Helpers
{
    static class AssemblyExtensions
    {
        public static FileVersionInfo FileVersion(this Assembly assembly)
            => FileVersionInfo.GetVersionInfo(assembly.Location);

        public static string ProductVersion(this Assembly assembly)
            => assembly.FileVersion().ProductVersion;
    }
}
