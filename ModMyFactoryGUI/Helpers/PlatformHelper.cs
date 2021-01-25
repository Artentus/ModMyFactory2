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
        private static readonly string[] EmptyExtensions = Array.Empty<string>();

        public static void OpenWebUrl(string url)
        {
#if WIN32
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
#elif LINUX
            Process.Start("xdg-open", url);
#elif OSX
            Process.Start("open", url);
#else
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
#endif
        }

        public static void OpenDirectory(DirectoryInfo directory)
        {
            string path = directory.FullName;
            if (!path.EndsWith(Path.DirectorySeparatorChar))
                path += Path.DirectorySeparatorChar;

#if WIN32
            Process.Start(new ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
#elif LINUX
            Process.Start("xdg-open", path);
#elif OSX
            Process.Start("open", path);
#else
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
#endif
        }

        public static Platform GetCurrentPlatform()
        {
#if WIN32
            return Platform.Win64Manual;
#elif LINUX
            return Platform.Linux64;
#elif OSX
            return Platform.OSX;
#else
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
#endif
        }

        public static string[] GetFactorioArchiveExtensions()
        {
#if WIN32
            return WindowsArchiveExtensions;
#elif LINUX
            return LinuxArchiveExtensions;
#elif OSX
            return MacArchiveExtensions;
#else
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
#endif
        }

        public static string[] GetSymbolicLinkExtensions()
        {
#if WIN32
            return WindowsSymbolicLinkExtensions;
#elif LINUX
            return EmptyExtensions;
#elif OSX
            return EmptyExtensions;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsSymbolicLinkExtensions;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LinuxSymbolicLinkExtensions;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return EmptyExtensions;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
#endif
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
#if WIN32
            Shell.CreateSymbolicLink(linkPath, targetPath, arguments, iconLocation);
#elif LINUX
            CreateUnixSymbolicLink(linkPath, targetPath, arguments);
#elif OSX
            CreateUnixSymbolicLink(linkPath, targetPath, arguments);
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Shell.CreateSymbolicLink(linkPath, targetPath, arguments, iconLocation);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // If the app is not standalone we need to create a shell script instead of an actual link
                CreateLinuxShellScript(linkPath, targetPath, arguments);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                CreateUnixSymbolicLink(linkPath, targetPath, arguments);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
#endif
        }
    }
}
