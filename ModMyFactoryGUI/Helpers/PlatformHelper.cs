//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi.Factorio;
using ModMyFactory.Win32;
using Mono.Unix;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ModMyFactoryGUI.Helpers
{
    internal static class PlatformHelper
    {
        private static readonly string[] WindowsArchiveExtensions = { "zip" };
        private static readonly string[] WindowsSymbolicLinkExtensions = { "lnk" };
        private static readonly string[] LinuxArchiveExtensions = { "tar.gz", "tar.xz" };
        private static readonly string[] LinuxSymbolicLinkExtensions = { "sh" };
        private static readonly string[] MacArchiveExtensions = { "dmg" };
        private static readonly string[] EmptyExtensions = new string[0];

        public static void OpenWebUrl(string url)
        {
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
        }

        public static void OpenDirectory(DirectoryInfo directory)
        {
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
        }

        public static Platform GetCurrentPlatform()
        {
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
        }

        public static string[] GetFactorioArchiveExtensions()
        {
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
        }

        public static string[] GetSymbolicLinkExtensions()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsSymbolicLinkExtensions;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
#if SELFCONTAINED
                return EmptyExtensions;
#else
                return LinuxSymbolicLinkExtensions;
#endif
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return EmptyExtensions;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        private static void CreateUnixSymbolicLink(string linkPath, string targetPath, string arguments)
        {
            var info = new UnixSymbolicLinkInfo(linkPath);
            info.CreateSymbolicLinkTo($"{targetPath} {arguments}");
        }

#if !SELFCONTAINED

        private static void CreateLinuxShellScript(string scriptPath, string targetPath, string arguments)
        {
            var file = new FileInfo(scriptPath);
            using var fs = file.Open(FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(fs);
            writer.WriteLine("#!/usr/bin/env bash");
            writer.WriteLine($"{targetPath} {arguments}");

            var info = new UnixFileInfo(file.FullName);
            info.FileAccessPermissions |= FileAccessPermissions.UserExecute | FileAccessPermissions.GroupExecute;
            info.Refresh();
        }

#endif

        public static void CreateSymbolicLink(string linkPath, string targetPath, string arguments, string iconLocation)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Shell.CreateSymbolicLink(linkPath, targetPath, arguments, iconLocation);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
#if SELFCONTAINED
                CreateUnixSymbolicLink(linkPath, targetPath, arguments);
#else
                // If the app is not standalone we need to create a shell script instead of an actual link
                CreateLinuxShellScript(linkPath, targetPath, arguments);
#endif
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                CreateUnixSymbolicLink(linkPath, targetPath, arguments);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
