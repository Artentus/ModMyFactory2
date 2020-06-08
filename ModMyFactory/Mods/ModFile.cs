//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.Mods
{
    public static class ModFile
    {
        internal static Stream LoadThumbnail(DirectoryInfo directory)
        {
            Stream thumbnail = null;
            var thumbnailFile = new FileInfo(Path.Combine(directory.FullName, "thumbnail.png"));
            if (thumbnailFile.Exists) thumbnail = thumbnailFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return thumbnail;
        }

        internal static async Task<Stream> CopyThumbnailAsync(Stream thumbnail)
        {
            if (thumbnail is null) return null;

            var newStream = new MemoryStream((int)thumbnail.Length);
            await thumbnail.CopyToAsync(newStream);

            newStream.Position = 0;
            return newStream;
        }

        internal static bool TryParseFileName(string fileName, out string name, out AccurateVersion version)
        {
            (name, version) = (null, default);

            int index = fileName.LastIndexOf('_');
            if ((index < 1) || (index >= fileName.Length - 1)) return false;

            name = fileName.Substring(0, index);
            if (string.IsNullOrWhiteSpace(name)) return false;

            var versionString = fileName.Substring(index + 1);
            return AccurateVersion.TryParse(versionString, out version);
        }

        /// <summary>
        /// Tries to load a mod file
        /// </summary>
        /// <param name="path">The path to load the mod from</param>
        public static async Task<(bool, IModFile)> TryLoadAsync(string path)
        {
            var file = new FileInfo(path);
            if (file.Exists) return await ZippedModFile.TryLoadAsync(file);

            var directory = new DirectoryInfo(path);
            if (directory.Exists) return await ExtractedModFile.TryLoadAsync(directory);

            return (false, null);
        }

        /// <summary>
        /// Tries to load a mod file
        /// </summary>
        /// <param name="fileSystemInfo">The path to load the mod from</param>
        public static async Task<(bool, IModFile)> TryLoadAsync(FileSystemInfo fileSystemInfo) => await TryLoadAsync(fileSystemInfo.FullName);

        /// <summary>
        /// Loads a mod file
        /// </summary>
        /// <param name="path">The path to load the mod from</param>
        public static async Task<IModFile> LoadAsync(string path)
        {
            var file = new FileInfo(path);
            if (file.Exists) return await ZippedModFile.LoadAsync(file);

            var directory = new DirectoryInfo(path);
            if (directory.Exists) return await ExtractedModFile.LoadAsync(directory);

            throw new PathNotFoundException("The specified path does not exist");
        }

        /// <summary>
        /// Loads a mod file
        /// </summary>
        /// <param name="fileSystemInfo">The path to load the mod from</param>
        public static async Task<IModFile> LoadAsync(FileSystemInfo fileSystemInfo) => await LoadAsync(fileSystemInfo.FullName);
    }
}
