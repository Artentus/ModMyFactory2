//  Copyright (C) 2020-2021 Mathis Rech
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
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.Export
{
    public sealed class ImportException : Exception
    {
        internal ImportException(string message, Exception? innerException = null)
            : base(message, innerException)
        { }
    }

    public static class Importer
    {
        private static bool InferArchiveFromExtension(FileInfo file)
        {
            string ext = file.Extension;
            if (string.Equals(ext, ".fmp", StringComparison.OrdinalIgnoreCase)) return false;
            else if (string.Equals(ext, ".fmpa", StringComparison.OrdinalIgnoreCase)) return true;
            else throw new ArgumentException("Unable to infer package type from file extension, specify manually instead", nameof(file));
        }

        private static bool InferArchiveFromExtension(string path)
        {
            string ext = Path.GetExtension(path);
            if (string.Equals(ext, ".fmp", StringComparison.OrdinalIgnoreCase)) return false;
            else if (string.Equals(ext, ".fmpa", StringComparison.OrdinalIgnoreCase)) return true;
            else throw new ArgumentException("Unable to infer package type from file extension, specify manually instead", nameof(path));
        }

        private static ImportResult ImportJson(string path)
        {
            string json = File.ReadAllText(path);
            var package = JsonConvert.DeserializeObject<Package>(json);
            if (package is null) throw new ImportException("File is not a valid modpack file");
            return new ImportResult(package);
        }

        private static async Task<ImportResult> ImportJsonAsync(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                                              4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

            var buffer = new byte[(int)stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            string json = Encoding.UTF8.GetString(buffer);

            var package = JsonConvert.DeserializeObject<Package>(json);
            if (package is null) throw new ImportException("File is not a valid modpack file");
            return new ImportResult(package);
        }

        private static ImportResult ImportArchive(FileInfo file, string tempPath)
        {
            var tempDir = new DirectoryInfo(tempPath);
            if (!tempDir.Exists) tempDir.Create();

            Package? package = null;
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

            if (package is null) throw new ImportException("File did not contain a valid package description");
            return new ImportResult(package, extractedFiles);
        }

        private static Task<ImportResult> ImportArchiveAsync(FileInfo file, string tempPath)
            => Task.Run(() => ImportArchive(file, tempPath));

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="file">The package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        /// <param name="archive">Whether the package to extract is of type FMPA</param>
        public static ImportResult Import(FileInfo file, string tempPath, bool archive)
        {
            if (archive) return ImportArchive(file, tempPath);
            else return ImportJson(file.FullName);
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="path">Path to the package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        /// <param name="archive">Whether the package to extract is of type FMPA</param>
        public static ImportResult Import(string path, string tempPath, bool archive)
        {
            if (archive) return ImportArchive(new FileInfo(path), tempPath);
            else return ImportJson(path);
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
            else return ImportJsonAsync(file.FullName);
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="path">Path to the package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        /// <param name="archive">Whether the package to extract is of type FMPA</param>
        public static Task<ImportResult> ImportAsync(string path, string tempPath, bool archive)
        {
            if (archive) return ImportArchiveAsync(new FileInfo(path), tempPath);
            else return ImportJsonAsync(path);
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="file">The package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        public static ImportResult Import(FileInfo file, string tempPath)
        {
            bool archive = InferArchiveFromExtension(file);
            return Import(file, tempPath, archive);
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="path">Path to the package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        public static ImportResult Import(string path, string tempPath)
        {
            bool archive = InferArchiveFromExtension(path);
            return Import(path, tempPath, archive);
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="file">The package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        public static Task<ImportResult> ImportAsync(FileInfo file, string tempPath)
        {
            bool archive = InferArchiveFromExtension(file);
            return ImportAsync(file, tempPath, archive);
        }

        /// <summary>
        /// Imports a mod package
        /// </summary>
        /// <param name="path">Path to the package file to import</param>
        /// <param name="tempPath">Temporary path to store extrated mod files (if applicable)</param>
        public static Task<ImportResult> ImportAsync(string path, string tempPath)
        {
            bool archive = InferArchiveFromExtension(path);
            return ImportAsync(path, tempPath, archive);
        }
    }
}
