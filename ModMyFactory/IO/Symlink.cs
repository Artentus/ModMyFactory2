//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.IO.Win32;
using System;

#if NETCORE

using System.Runtime.InteropServices;
using ModMyFactory.IO.Unix;

#endif

namespace ModMyFactory.IO
{
    internal static class Symlink
    {
        public static ISymlinkInfo FromPath(string path)
        {
#if NETFULL
            return new JunctionInfo(path);
#elif NETCORE
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
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}
