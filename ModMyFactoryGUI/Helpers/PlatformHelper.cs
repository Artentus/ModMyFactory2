//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi.Factorio;
using System.Diagnostics;
using System.IO;

#if NETCORE

using System;
using System.Runtime.InteropServices;

#endif

namespace ModMyFactoryGUI.Helpers
{
    internal static class PlatformHelper
    {
        private static readonly string[] WindowsArchiveExtensions = { "zip" };
#if NETCORE
        private static readonly string[] LinuxArchiveExtensions = { "tar.gz", "tar.xz" };
        private static readonly string[] MacArchiveExtensions = { "dmg" };
#endif

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

        public static void OpenDirectory(DirectoryInfo directory)
        {
#if NETFULL
            string path = directory.FullName;
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path += Path.DirectorySeparatorChar;

            Process.Start(new ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
#elif NETCORE
            string path = directory.FullName;
            if (!path.EndsWith(Path.DirectorySeparatorChar))
                path += Path.DirectorySeparatorChar;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", path);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public static Platform GetCurrentPlatform()
        {
#if NETFULL
            return Platform.Win64Manual;
#elif NETCORE
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Platform.Win64Manual;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Platform.Linux64;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Platform.OSX;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public static string[] GetFactorioArchiveExtensions()
        {
#if NETFULL
            return WindowsArchiveExtensions;
#elif NETCORE
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsArchiveExtensions;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LinuxArchiveExtensions;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return MacArchiveExtensions;
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
