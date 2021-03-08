//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.IO.Win32;
using System;

using System.Runtime.InteropServices;
using ModMyFactory.IO.Unix;

namespace ModMyFactory.IO
{
    internal static class Symlink
    {
        public static ISymlinkInfo FromPath(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new JunctionInfo(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new SymlinkInfo(path);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
