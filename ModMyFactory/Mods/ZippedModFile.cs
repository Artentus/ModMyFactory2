//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.IO;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Readers.Zip;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.Mods
{
    public class ZippedModFile : IModFile
    {
        private FileInfo _file;

        public string FilePath => _file.FullName;

        public ModInfo Info { get; }

        public Stream Thumbnail { get; }

        public bool IsExtracted => false;

        public bool Enabled
        {
            get => string.Equals(_file.Extension, ".zip", StringComparison.InvariantCultureIgnoreCase);
            set
            {
                if (value != Enabled)
                {
                    if (value)
                        _file.Rename(string.Concat(Info.Name, "_", Info.Version, ".zip"));
                    else
                        _file.Rename(string.Concat(Info.Name, "_", Info.Version, ".disabled"));
                }
            }
        }

        internal ZippedModFile(FileInfo file, ModInfo info, Stream thumbnail)
            => (_file, Info, Thumbnail) = (file, info, thumbnail);

        ~ZippedModFile()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Thumbnail?.Dispose();
        }

        /// <summary>
        /// Tries to load a mod file
        /// </summary>
        /// <param name="file">The archive file to load</param>
        public static Task<(bool, ZippedModFile)> TryLoadAsync(FileInfo file)
        {
            return Task.Run(() =>
            {
                if (!file.Exists) return (false, null);

                bool hasInfo = false;
                ModInfo info = default;
                Stream thumbnail = null;
                try
                {
                    using var fs = file.OpenRead();
                    using var reader = ZipReader.Open(fs);
                    while (reader.MoveToNextEntry())
                    {
                        var entry = reader.Entry;
                        if (!entry.IsDirectory)
                        {
                            string key = entry.Key.TrimStart('/');
                            if (key.IndexOf('/') == key.LastIndexOf('/')) // All top level files
                            {
                                if (key.EndsWith("info.json"))
                                {
                                    using var stream = reader.OpenEntryStream();
                                    using var sr = new StreamReader(stream, Encoding.UTF8);
                                    string json = sr.ReadToEnd();

                                    info = ModInfo.FromJson(json);
                                    hasInfo = true;
                                }
                                else if (key.EndsWith("thumbnail.png"))
                                {
                                    thumbnail = new MemoryStream();
                                    reader.WriteEntryTo(thumbnail);
                                    thumbnail.Position = 0;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    thumbnail?.Dispose(); // In case of exception dispose thumbnail stream
                    if (ex is IOException) throw; // Catch all but file system exceptions
                    else return (false, null); // File was not valid
                }

                if (!ModFile.TryParseFileName(file.NameWithoutExtension(), out var fileName, out var fileVersion)
                    || (fileName != info.Name) || (fileVersion != info.Version))
                {
                    thumbnail?.Dispose();
                    return (false, null);
                }

                return (hasInfo, hasInfo ? new ZippedModFile(file, info, thumbnail) : null);
            });
        }

        /// <summary>
        /// Tries to load a mod file
        /// </summary>
        /// <param name="path">The path to an archive file to load</param>
        public static Task<(bool, ZippedModFile)> TryLoadAsync(string path)
        {
            var file = new FileInfo(path);
            return TryLoadAsync(file);
        }

        /// <summary>
        /// Loads a mod file
        /// </summary>
        /// <param name="file">The archive file to load</param>
        public static async Task<ZippedModFile> LoadAsync(FileInfo file)
        {
            if (!file.Exists) throw new PathNotFoundException("The specified file does not exist");

            (bool success, var result) = await TryLoadAsync(file);
            if (!success) throw new InvalidModDataException("The specified file is not a valid mod");
            return result;
        }

        /// <summary>
        /// Loads a mod file
        /// </summary>
        /// <param name="path">The path to an archive file to load</param>
        public static async Task<ZippedModFile> LoadAsync(string path)
        {
            var file = new FileInfo(path);
            return await LoadAsync(file);
        }

        public void Delete()
        {
            Thumbnail?.Dispose();
            _file.Delete();
        }

        public async Task<IModFile> CopyToAsync(string destination)
        {
            var fullPath = Path.Combine(destination, _file.Name);
            var newFile = await _file.CopyToAsync(fullPath);
            var newThumbnail = await ModFile.CopyThumbnailAsync(Thumbnail);
            return new ZippedModFile(newFile, Info, newThumbnail);
        }

        public async Task MoveToAsync(string destination)
        {
            var fullPath = Path.Combine(destination, _file.Name);
            _file = await _file.MoveToAsync(fullPath);
        }

        /// <summary>
        /// Extracts this mod file
        /// </summary>
        /// <param name="destination">The path to extract this mod file to, excluding the mod files name itself</param>
        public async Task<ExtractedModFile> ExtractToAsync(string destination)
        {
            await Task.Run(() =>
            {
                using var fs = _file.OpenRead();
                using var reader = ZipReader.Open(fs);
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                        reader.WriteEntryToDirectory(destination, new ExtractionOptions() { ExtractFullPath = true });
                }
            });

            var newDir = new DirectoryInfo(Path.Combine(destination, _file.NameWithoutExtension()));
            var newThumbnail = await ModFile.LoadThumbnail(newDir);
            var extractedFile = new ExtractedModFile(newDir, Info, newThumbnail);

            if (!Enabled) extractedFile.Enabled = false;
            return extractedFile;
        }

        /// <summary>
        /// Extracts this mod file to the same location
        /// </summary>
        public async Task<ExtractedModFile> ExtractAsync() => await ExtractToAsync(_file.Directory.FullName);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
