//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Mods;
using System.IO;
using System.Threading.Tasks;
#if NETCORE
using System;
#endif

namespace ModMyFactory.Game
{
    public static class Factorio
    {
        static Steam _steam = null;

        static async Task<(bool, IModFile)> TryLoadCoreModAsync(DirectoryInfo directory)
        {
            var coreModPath = Path.Combine(directory.FullName, "data", "core");
            return await ExtractedModFile.TryLoadAsync(coreModPath);
        }

        static async Task<(bool, IModFile)> TryLoadBaseModAsync(DirectoryInfo directory)
        {
            var baseModPath = Path.Combine(directory.FullName, "data", "base");
            return await ExtractedModFile.TryLoadAsync(baseModPath);
        }

        static bool TryLoadExecutable(DirectoryInfo directory, out FileInfo executable)
        {
#if NETFULL
            executable = new FileInfo(Path.Combine(directory.FullName, "bin", "x64", "factorio.exe"));
            return executable.Exists;
#elif NETCORE
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT)
                executable = new FileInfo(Path.Combine(directory.FullName, "bin", "x64", "factorio.exe"));
            else if (os.Platform == PlatformID.Unix)
                executable = new FileInfo(Path.Combine(directory.FullName, "bin", "x64", "factorio"));
            else
                throw new PlatformException();
            return executable.Exists;
#endif
        }

        /// <summary>
        /// Tries to load a Factorio instance.
        /// </summary>
        /// <param name="directory">The directory the instance is stored in.</param>
        public static async Task<(bool, IFactorioInstance)> TryLoadAsync(DirectoryInfo directory)
        {
            if (!directory.Exists) return (false, null);
            if (!TryLoadExecutable(directory, out var executable)) return (false, null);

            (bool s1, var coreMod) = await TryLoadCoreModAsync(directory);
            if (!s1) return (false, null);

            (bool s2, var baseMod) = await TryLoadBaseModAsync(directory);
            if (!s2) return (false, null);

            return (true, new FactorioStandaloneInstance(directory, coreMod, baseMod, executable));
        }

        /// <summary>
        /// Tries to load a Factorio instance.
        /// </summary>
        /// <param name="directory">The path the instance is stored at.</param>
        public static async Task<(bool, IFactorioInstance)> TryLoadAsync(string path)
        {
            var dir = new DirectoryInfo(path);
            return await TryLoadAsync(dir);
        }

        /// <summary>
        /// Loads a Factorio instance.
        /// </summary>
        /// <param name="directory">The directory the instance is stored in.</param>
        public static async Task<IFactorioInstance> LoadAsync(DirectoryInfo directory)
        {
            (bool success, var result) = await TryLoadAsync(directory);
            if (!success) throw new InvalidPathException("The directory does not contain a valid Factorio instance.");
            return result;
        }

        /// <summary>
        /// Loads a Factorio instance.
        /// </summary>
        /// <param name="directory">The path the instance is stored at.</param>
        public static async Task<IFactorioInstance> LoadAsync(string path)
        {
            var dir = new DirectoryInfo(path);
            return await LoadAsync(dir);
        }

        static async Task<(bool, DirectoryInfo)> TryGetSteamDirectoryAsync(Steam steam)
        {
            var libraries = await steam.GetLibrariesAsync();
            foreach (var library in libraries)
            {
                foreach (var dir in library.EnumerateDirectories("Factorio"))
                    if (TryLoadExecutable(dir, out _)) return (true, dir);
            }
            return (false, null);
        }

        /// <summary>
        /// Tries to load the Factorio Steam instance.
        /// </summary>
        public static async Task<(bool, IFactorioInstance)> TryLoadSteamAsync()
        {
            if ((_steam is null) && !Steam.TryLoad(out _steam)) return (false, null); // Use same steam instance at all times
            (bool s0, var directory) = await TryGetSteamDirectoryAsync(_steam);
            if (!s0) return (false, null);

            (bool s1, var coreMod) = await TryLoadCoreModAsync(directory);
            if (!s1) return (false, null);

            (bool s2, var baseMod) = await TryLoadBaseModAsync(directory);
            if (!s2) return (false, null);

            return (true, new FactorioSteamInstance(directory, coreMod, baseMod, _steam));
        }

        /// <summary>
        /// Loads the Factorio Steam instance.
        /// </summary>
        public static async Task<IFactorioInstance> LoadSteamAsync()
        {
            (bool success, var result) = await TryLoadSteamAsync();
            if (!success) throw new ManagerException("Factorio Steam version not found.");
            return result;
        }
    }
}
