using System;
using ModMyFactory.IO.Win32;

#if NETCORE
using ModMyFactory.IO.Unix;
#endif

namespace ModMyFactory.IO
{
    static class Symlink
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
#endif
            throw new NotSupportedException("Current platform not supported.");
        }
    }
}
