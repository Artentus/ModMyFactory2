//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.IO;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.Mods
{
    public class ExtractedModFile : IModFile
    {
        private DirectoryInfo _directory;

        public string FilePath => _directory.FullName;

        public ModInfo Info { get; }

        public Stream Thumbnail { get; }

        public bool IsExtracted => true;

        public bool Enabled
        {
            get => File.Exists(Path.Combine(_directory.FullName, "info.json"));
            set
            {
                if (value != Enabled)
                {
                    if (value)
                    {
                        var infoFile = new FileInfo(Path.Combine(_directory.FullName, "info.disabled"));
                        infoFile.Rename("info.json");
                    }
                    else
                    {
                        var infoFile = new FileInfo(Path.Combine(_directory.FullName, "info.json"));
                        infoFile.Rename("info.disabled");
                    }
                }
            }
        }

        internal ExtractedModFile(DirectoryInfo directory, ModInfo info, Stream thumbnail)
            => (_directory, Info, Thumbnail) = (directory, info, thumbnail);

        ~ExtractedModFile()
        {
            Dispose(false);
        }

        private void PopulateZipArchive(ZipWriter writer, DirectoryInfo directory, string path)
        {
            foreach (var file in directory.EnumerateFiles())
                writer.Write(path + "/" + file.Name, file);

            foreach (var subDir in directory.EnumerateDirectories())
                PopulateZipArchive(writer, subDir, path + "/" + subDir.Name);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Thumbnail?.Dispose();
        }

        /// <summary>
        /// Tries to load a mod file
        /// </summary>
        /// <param name="directory">The directory to load</param>
        public static async Task<(bool, ExtractedModFile)> TryLoadAsync(DirectoryInfo directory)
        {
            var infoFile = new FileInfo(Path.Combine(directory.FullName, "info.json"));
            if (!infoFile.Exists)
            {
                infoFile = new FileInfo(Path.Combine(directory.FullName, "info.disabled"));
                if (!infoFile.Exists) return (false, null);
            }

            ModInfo info;
            try
            {
                info = await ModInfo.FromFileAsync(infoFile);
            }
            catch
            {
                return (false, null);
            }


            // Hardcoded exceptions for core and base mods
            if ((directory.Name != "core") && (directory.Name != "base"))
            {
                if (!ModFile.TryParseFileName(directory.Name, out var fileName, out var fileVersion)) return (false, null);
                if ((fileName != info.Name) || (fileVersion != info.Version)) return (false, null);
            }

            var thumbnail = ModFile.LoadThumbnail(directory);
            return (true, new ExtractedModFile(directory, info, thumbnail));
        }

        /// <summary>
        /// Tries to load a mod file
        /// </summary>
        /// <param name="path">The path to a directory to load</param>
        public static async Task<(bool, ExtractedModFile)> TryLoadAsync(string path)
        {
            var dir = new DirectoryInfo(path);
            return await TryLoadAsync(dir);
        }

        /// <summary>
        /// Loads a mod file
        /// </summary>
        /// <param name="directory">The directory to load</param>
        public static async Task<ExtractedModFile> LoadAsync(DirectoryInfo directory)
        {
            (bool success, var result) = await TryLoadAsync(directory);
            if (!success) throw new InvalidPathException("The path does not point to a valid mod file.");
            return result;
        }

        /// <summary>
        /// Loads a mod file
        /// </summary>
        /// <param name="path">The path to a directory to load</param>
        public static async Task<ExtractedModFile> LoadAsync(string path)
        {
            var dir = new DirectoryInfo(path);
            return await LoadAsync(dir);
        }

        public void Delete()
        {
            Thumbnail?.Dispose();
            _directory.Delete(true);
        }

        public async Task<IModFile> CopyToAsync(string destination)
        {
            var fullPath = Path.Combine(destination, _directory.Name);
            var newDir = await _directory.CopyToAsync(fullPath);
            var newThumbnail = ModFile.LoadThumbnail(newDir);
            return new ExtractedModFile(newDir, Info, newThumbnail);
        }

        public async Task MoveToAsync(string destination)
        {
            var fullPath = Path.Combine(destination, _directory.Name);
            _directory = await _directory.MoveToAsync(fullPath);
        }

        /// <summary>
        /// Packs this mod file
        /// </summary>
        /// <param name="destination">The path to pack this mod file at, excluding the mod files name itself</param>
        public async Task<ZippedModFile> PackAsync(string destination)
        {
            bool wasEnabled = Enabled;
            if (!wasEnabled) Enabled = true;

            var newFile = new FileInfo(Path.Combine(destination, _directory.Name + ".zip"));
            if (!newFile.Directory.Exists) newFile.Directory.Create();

            await Task.Run(() =>
            {
                using var fs = newFile.Open(FileMode.Create, FileAccess.Write);
                using var writer = new ZipWriter(fs, new ZipWriterOptions(CompressionType.Deflate));
                PopulateZipArchive(writer, _directory, _directory.Name);
            });

            var newThumbnail = await ModFile.CopyThumbnailAsync(Thumbnail);
            var zippedFile = new ZippedModFile(newFile, Info, newThumbnail);

            if (!wasEnabled)
            {
                zippedFile.Enabled = false;
                Enabled = false;
            }
            return zippedFile;
        }

        /// <summary>
        /// Packs this mod file in the same location
        /// </summary>
        public async Task<ZippedModFile> PackAsync() => await PackAsync(_directory.Parent.FullName);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
