//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using SharpCompress.Readers;
using SharpCompress.Readers.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.Export
{
    public static class Importer
    {
        private static async Task<ImportResult> ImportJsonAsync(FileInfo file)
        {
            using var stream = file.OpenRead();
            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();
            var package = JsonConvert.DeserializeObject<Package>(json);
            return new ImportResult(package);
        }

        private static Task<ImportResult> ImportArchiveAsync(FileInfo file, string tempPath)
        {
            return Task.Run(() =>
            {
                var tempDir = new DirectoryInfo(tempPath);
                if (!tempDir.Exists) tempDir.Create();

                Package package = null;
                var extractedFiles = new List<FileInfo>();

                using var stream = file.OpenRead();
                using var reader = ZipReader.Open(stream);
                while (reader.MoveToNextEntry())
                {
                    if (string.Equals(reader.Entry.Key, "pack.json", StringComparison.OrdinalIgnoreCase))
                    {
                        using var jsonStream = new MemoryStream((int)reader.Entry.Size);
                        reader.WriteEntryTo(jsonStream);
                        jsonStream.Position = 0;

                        using var jsonReader = new StreamReader(jsonStream);
                        string json = jsonReader.ReadToEnd();
                        package = JsonConvert.DeserializeObject<Package>(json);
                    }
                    else
                    {
                        string path = Path.Combine(tempPath, reader.Entry.Key);
                        var extFile = new FileInfo(path);
                        reader.WriteEntryTo(extFile);
                        extractedFiles.Add(extFile);
                    }
                }

                if (package is null) throw new InvalidOperationException("File did not contain a valid package description");
                return new ImportResult(package, extractedFiles);
            });
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="file">The package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        /// <param name="archive">Whether the package to extract is of type FMPA</param>
        public static Task<ImportResult> ImportAsync(FileInfo file, string tempPath, bool archive)
        {
            if (archive) return ImportArchiveAsync(file, tempPath);
            else return ImportJsonAsync(file);
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="path">Path to the package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        /// <param name="archive">Whether the package to extract is of type FMPA</param>
        public static Task<ImportResult> ImportAsync(string path, string tempPath, bool archive)
        {
            var file = new FileInfo(path);
            if (archive) return ImportArchiveAsync(file, tempPath);
            else return ImportJsonAsync(file);
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="file">The package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        public static Task<ImportResult> ImportAsync(FileInfo file, string tempPath)
        {
            if (string.Equals(file.Extension, ".fmp", StringComparison.OrdinalIgnoreCase)) return ImportJsonAsync(file);
            else if (string.Equals(file.Extension, ".fmpa", StringComparison.OrdinalIgnoreCase)) return ImportArchiveAsync(file, tempPath);
            else throw new ArgumentException("Unable to infer package type from file extension, specify manually instead", nameof(file));
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="path">Path to the package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        public static Task<ImportResult> ImportAsync(string path, string tempPath)
        {
            var file = new FileInfo(path);
            if (string.Equals(file.Extension, ".fmp", StringComparison.OrdinalIgnoreCase)) return ImportJsonAsync(file);
            else if (string.Equals(file.Extension, ".fmpa", StringComparison.OrdinalIgnoreCase)) return ImportArchiveAsync(file, tempPath);
            else throw new ArgumentException("Unable to infer package type from file extension, specify manually instead", nameof(path));
        }
    }
}
