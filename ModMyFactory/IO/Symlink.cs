//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.IO.Win32;

#if NETCORE

using ModMyFactory.IO.Unix;
using System;

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
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT)
                return new JunctionInfo(path);
            else if (os.Platform == PlatformID.Unix)
                return new SymlinkInfo(path);
            else
                throw new PlatformException();
#endif
        }
    }
}
