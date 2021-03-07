//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Mods;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SharpCompress.Readers.Zip;
using SharpCompress.Readers.Tar;
using SharpCompress.Compressors.Xz;

namespace ModMyFactory.Game
{
    public static class Factorio
    {
        private static Steam? _steam = null;

        private static async Task<(bool, IModFile?)> TryLoadCoreModAsync(DirectoryInfo directory)
        {
            var coreModPath = Path.Combine(directory.FullName, "data", "core");
            return await ExtractedModFile.TryLoadAsync(coreModPath);
        }

        private static async Task<(bool, IModFile?)> TryLoadBaseModAsync(DirectoryInfo directory)
        {
            var baseModPath = Path.Combine(directory.FullName, "data", "base");
            return await ExtractedModFile.TryLoadAsync(baseModPath);
        }

        private static bool TryLoadExecutable(DirectoryInfo directory, out FileInfo executable)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                executable = new FileInfo(Path.Combine(directory.FullName, "bin", "x64", "factorio.exe"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                executable = new FileInfo(Path.Combine(directory.FullName, "bin", "x64", "factorio"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                executable = new FileInfo(Path.Combine(directory.FullName, "MacOS", "factorio"));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return executable.Exists;
        }

        /// <summary>
        /// Tries to load a Factorio instance
        /// </summary>
        /// <param name="directory">The directory the instance is stored in</param>
        public static async Task<(bool, IFactorioInstance?)> TryLoadAsync(DirectoryInfo directory)
        {
            var dir = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? new DirectoryInfo(Path.Combine(directory.FullName, "factorio.app", "Contents"))
                : directory;

            if (!dir.Exists) return (false, null);
            if (!TryLoadExecutable(dir, out var executable)) return (false, null);

            (bool s1, var coreMod) = await TryLoadCoreModAsync(dir);
            if (!s1) return (false, null);

            (bool s2, var baseMod) = await TryLoadBaseModAsync(dir);
            if (!s2) return (false, null);

            return (true, new FactorioStandaloneInstance(directory, coreMod!, baseMod!, executable));
        }

        /// <summary>
        /// Tries to load a Factorio instance
        /// </summary>
        /// <param name="path">The path the instance is stored at</param>
        public static async Task<(bool, IFactorioInstance?)> TryLoadAsync(string path)
        {
            var dir = new DirectoryInfo(path);
            return await TryLoadAsync(dir);
        }

        /// <summary>
        /// Loads a Factorio instance
        /// </summary>
        /// <param name="directory">The directory the instance is stored in</param>
        public static async Task<IFactorioInstance> LoadAsync(DirectoryInfo directory)
        {
            if (directory.Exists) throw new PathNotFoundException("The specified directory does not exist");

            (bool success, var result) = await TryLoadAsync(directory);
            if (!success) throw new InvalidFactorioDataException("The directory does not contain a valid Factorio instance");
            return result!;
        }

        /// <summary>
        /// Loads a Factorio instance
        /// </summary>
        /// <param name="path">The path the instance is stored at</param>
        public static async Task<IFactorioInstance> LoadAsync(string path)
        {
            var dir = new DirectoryInfo(path);
            return await LoadAsync(dir);
        }

        private static async Task<(bool, DirectoryInfo?)> TryGetSteamDirectoryAsync(Steam steam)
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
        /// Tries to load the Factorio Steam instance
        /// </summary>
        public static async Task<(bool, IFactorioInstance?)> TryLoadSteamAsync()
        {
            if ((_steam is null) && !Steam.TryLoad(out _steam)) return (false, null); // Use same steam instance at all times
            (bool s0, var directory) = await TryGetSteamDirectoryAsync(_steam);
            if (!s0) return (false, null);

            (bool s1, var coreMod) = await TryLoadCoreModAsync(directory!);
            if (!s1) return (false, null);

            (bool s2, var baseMod) = await TryLoadBaseModAsync(directory!);
            if (!s2) return (false, null);

            return (true, new FactorioSteamInstance(directory!, coreMod!, baseMod!, _steam));
        }

        /// <summary>
        /// Loads the Factorio Steam instance
        /// </summary>
        public static async Task<IFactorioInstance> LoadSteamAsync()
        {
            (bool success, var result) = await TryLoadSteamAsync();
            if (!success) throw new SteamInstanceNotFoundException("Factorio Steam version not found");
            return result!;
        }

        private static string GetTopDirectory(string path)
        {
            string current = path;
            string next = current;

            while (!string.IsNullOrEmpty(next))
            {
                current = next;
                next = Path.GetDirectoryName(current);
            }

            return current;
        }

        private static (bool, string?) Extract(IReader reader, string destination)
        {
            string? topLevelDir = null;
            while (reader.MoveToNextEntry())
            {
                var entry = reader.Entry;

                // All files in a valid Factorio archive must reside
                // in a top-level folder called 'Factorio_*'. If we
                // find a file that doesn't we can stop immediately.
                if (topLevelDir is null)
                {
                    topLevelDir = GetTopDirectory(entry.Key.TrimStart('/', '\\'));
                    topLevelDir = topLevelDir.TrimEnd('/', '\\');

                    if (!topLevelDir.StartsWith("factorio", StringComparison.OrdinalIgnoreCase))
                        return (false, topLevelDir);
                }
                else
                {
                    string dir = GetTopDirectory(entry.Key.TrimStart('/', '\\'));
                    dir = dir.TrimEnd('/', '\\');
                    if (!string.Equals(dir, topLevelDir, StringComparison.OrdinalIgnoreCase))
                        return (false, topLevelDir);
                }

                if (!entry.IsDirectory)
                    reader.WriteEntryToDirectory(destination, new ExtractionOptions() { ExtractFullPath = true });
            }

            return (!string.IsNullOrEmpty(topLevelDir), topLevelDir);
        }

        private static (bool, string?) ExtractWin32(FileInfo archiveFile, string destination)
        {
            using var stream = archiveFile.OpenRead();
            using var reader = ZipReader.Open(stream);
            return Extract(reader, destination);
        }

        private static (bool, string?) ExtractLinux(FileInfo archiveFile, string destination)
        {
            using var stream = archiveFile.OpenRead();
            bool isXz = XZStream.IsXZStream(stream);
            stream.Position = 0;
            if (isXz)
            {
                using var xzStream = new XZStream(stream);
                using var reader = TarReader.Open(xzStream);
                return Extract(reader, destination);
            }
            else
            {
                using var reader = TarReader.Open(stream);
                return Extract(reader, destination);
            }
        }

        private static (bool, string?) ExtractMac(FileInfo archiveFile, string destination)
        {
            // ToDo: implement once SharpCompress releases DMG support
            return (false, null);
        }

        /// <summary>
        /// Tries to extract an archive containing Factorio
        /// </summary>
        /// <param name="archiveFile">The file to extract</param>
        /// <param name="destination">Where to extract to</param>
        /// <param name="dirName">Optional<br/>The name of the top level directory of the resulting instance, if successfull</param>
        public static async Task<(bool, IFactorioInstance?)> TryExtract(FileInfo archiveFile, string destination, string? dirName = null)
        {
            var destinationDir = new DirectoryInfo(destination);
            if (!destinationDir.Exists) destinationDir.Create();

            var (valid, extractName) = await Task.Run(() =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return ExtractWin32(archiveFile, destination);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return ExtractLinux(archiveFile, destination);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return ExtractMac(archiveFile, destination);
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            });

            if (!valid)
            {
                if (!string.IsNullOrEmpty(extractName))
                {
                    // We may have extracted some files already, but they must
                    // all reside in a directory called 'Factorio_*' so we can
                    // clean up easily.
                    var dir = destinationDir.EnumerateDirectories(extractName).FirstOrDefault();
                    if (!(dir is null)) dir.Delete(true);
                }

                return (false, null);
            }
            else
            {
                var dir = destinationDir.EnumerateDirectories(extractName).First(); // Must exist
                if (!string.IsNullOrEmpty(dirName))
                    dir.MoveTo(Path.Combine(dir.Parent.FullName, dirName));

                var (success, instance) = await TryLoadAsync(dir);
                if (!success) dir.Delete();
                return (success, instance);
            }
        }
    }
}
