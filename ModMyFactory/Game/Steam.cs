using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ModMyFactory.Game
{
    class Steam
    {
        static bool TryGetSteamPathWin32(out string path)
        {
            RegistryKey softwareKey = null;
            try
            {
                string softwarePath = Environment.Is64BitProcess ? @"SOFTWARE\WOW6432Node" : "SOFTWARE";
                softwareKey = Registry.LocalMachine.OpenSubKey(softwarePath, false);

                using (var key = softwareKey.OpenSubKey(@"Valve\Steam"))
                {
                    var obj = key.GetValue("InstallPath");
                    path = obj as string;
                    return Directory.Exists(path);
                }
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

        static bool TryGetSteamPathUnix(out string path)
        {
            // Default path when installing through packet manager. Custom paths not supported.
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam");
            return Directory.Exists(path);
        }

        static bool TryGetSteamPath(out string path)
        {
#if NETFULL
            return TryGetSteamPathWin32(out path);
#elif NETCORE
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT)
                return TryGetSteamPathWin32(out path);
            else if (os.Platform == PlatformID.Unix)
                return TryGetSteamPathUnix(out path);
            else
                throw new PlatformException();
#endif
        }

        static DirectoryInfo GetLibrary(string basePath)
            => new DirectoryInfo(Path.Combine(basePath, "steamapps", "common"));

        /// <summary>
        /// Tries to load Steam.
        /// </summary>
        /// <param name="steam">Out. The instance of Steam on this system.</param>
        public static bool TryLoad(out Steam steam)
        {
            steam = null;
            if (!TryGetSteamPath(out var path)) return false;
            steam = new Steam(path);
            return true;
        }


        readonly string _path;

        private Steam(string path)
        {
            _path = path;
        }

        async Task<List<string>> ReadLibraryPathsAsync()
        {
            var libraryPaths = new List<string>();

            var vdfFile = new FileInfo(Path.Combine(_path, "steamapps", "libraryfolders.vdf"));
            if (!vdfFile.Exists) return libraryPaths;

            string content;
            using (var stream = vdfFile.OpenRead())
            {
                using (var reader = new StreamReader(stream))
                    content = await reader.ReadToEndAsync();
            }

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
        /// Gets a list of app libraries on the system.
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
        /// Starts a Steam app.
        /// </summary>
        /// <param name="app">The app to start.</param>
        /// <param name="args">Optional arguments.</param>
        public void StartApp(SteamApp app, params string[] args)
        {
#if NETFULL
            var startInfo = new ProcessStartInfo(Path.Combine(_path, "Steam.exe"));
#elif NETCORE
            ProcessStartInfo startInfo;
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT)
                startInfo = new ProcessStartInfo(Path.Combine(_path, "Steam.exe"));
            else if (os.Platform == PlatformID.Unix)
                startInfo = new ProcessStartInfo("steam");
            else
                throw new PlatformException();
#endif
            string argString = $"-applaunch {((long)app).ToString()}";
            if (!(args is null) && (args.Length != 0))
                argString += " " + string.Join(" ", args);
            startInfo.Arguments = argString;

            Process.Start(startInfo);
        }
    }
}
