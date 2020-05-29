//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#if NETCORE

using System.Runtime.InteropServices;

#endif

namespace ModMyFactory.Game
{
    internal class Steam
    {
        private static bool TryGetSteamPathWin32(out string path)
        {
            RegistryKey softwareKey = null;
            try
            {
                string softwarePath = Environment.Is64BitProcess ? @"SOFTWARE\WOW6432Node" : "SOFTWARE";
                softwareKey = Registry.LocalMachine.OpenSubKey(softwarePath, false);

                using var key = softwareKey.OpenSubKey(@"Valve\Steam");
                var obj = key.GetValue("InstallPath");
                path = obj as string;
                return Directory.Exists(path);
            }
            catch
            {
                path = null;
                return false;
            }
            finally
            {
                softwareKey?.Close();
            }
        }

        private static bool TryGetSteamPathLinux(out string path)
        {
            // Default path when installing through packet manager. Custom paths not supported.
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam");
            return Directory.Exists(path);
        }

        private static bool TryGetSteamPath(out string path)
        {
#if NETFULL
            return TryGetSteamPathWin32(out path);
#elif NETCORE
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TryGetSteamPathWin32(out path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return TryGetSteamPathLinux(out path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Mac not (yet) supported, but we exit gracefully since it is supported in other places
                path = null;
                return false;
            }
            else
            {
                throw new PlatformException();
            }
#else
            throw new PlatformException();
#endif
        }

        private static DirectoryInfo GetLibrary(string basePath)
            => new DirectoryInfo(Path.Combine(basePath, "steamapps", "common"));

        /// <summary>
        /// Tries to load Steam
        /// </summary>
        /// <param name="steam">Out<br/>The instance of Steam on this system</param>
        public static bool TryLoad(out Steam steam)
        {
            steam = null;
            if (!TryGetSteamPath(out var path)) return false;
            steam = new Steam(path);
            return true;
        }


        private readonly string _path;

        private Steam(string path)
        {
            _path = path;
        }

        private async Task<List<string>> ReadLibraryPathsAsync()
        {
            var libraryPaths = new List<string>();

            var vdfFile = new FileInfo(Path.Combine(_path, "steamapps", "libraryfolders.vdf"));
            if (!vdfFile.Exists) return libraryPaths;

            string content;
            using var stream = vdfFile.OpenRead();
            using var reader = new StreamReader(stream);
            content = await reader.ReadToEndAsync();

            var matches = Regex.Matches(content, "\"\\d\"\\s+\"(?<path>.+)\"");
            foreach (Match match in matches)
            {
                string path = match.Groups["path"].Value;
                path = path.Replace(@"\\", @"\");
                libraryPaths.Add(path);
            }

            return libraryPaths;
        }

        /// <summary>
        /// Gets a list of app libraries on the system
        /// </summary>
        public async Task<List<DirectoryInfo>> GetLibrariesAsync()
        {
            var libraries = new List<DirectoryInfo>();
            var mainDir = GetLibrary(_path);
            if (mainDir.Exists) libraries.Add(mainDir);

            var libraryPaths = await ReadLibraryPathsAsync();
            foreach (var path in libraryPaths)
            {
                var dir = GetLibrary(path);
                if (dir.Exists) libraries.Add(dir);
            }

            return libraries;
        }

        /// <summary>
        /// Starts a Steam app
        /// </summary>
        /// <param name="app">The app to start</param>
        /// <param name="arguments">Optional arguments</param>
        public void StartApp(SteamApp app, string arguments)
        {
#if NETFULL
            var startInfo = new ProcessStartInfo(Path.Combine(_path, "Steam.exe"));
#elif NETCORE
            ProcessStartInfo startInfo;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                startInfo = new ProcessStartInfo(Path.Combine(_path, "Steam.exe"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo = new ProcessStartInfo("steam");
            }
            else
            {
                throw new PlatformException();
            }
#else
            throw new PlatformException();
#endif
            var builder = new ArgumentBuilder();
            builder.AppendArgument("-applaunch");
            builder.AppendArgument(((long)app).ToString());
            builder.AppendExisting(arguments);
            startInfo.Arguments = builder.ToString();

            Process.Start(startInfo);
        }
    }
}
