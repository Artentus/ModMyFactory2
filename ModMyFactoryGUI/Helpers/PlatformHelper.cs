//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ModMyFactoryGUI.Helpers
{
    internal static class PlatformHelper
    {
        public static void OpenWebUrl(string url)
        {
#if NETFULL
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
#elif NETCORE
            // Code according to Microsoft
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
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
