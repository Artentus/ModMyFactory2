//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using SharpCompress.Common;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.Export
{
    /// <summary>
    /// Exports packages into FMP and FMPA files
    /// </summary>
    public sealed class Exporter
    {
        /// <summary>
        /// The package that will be exported
        /// </summary>
        public Package Package { get; }

        /// <summary>
        /// Whether the Exporter will create an FMPA file
        /// </summary>
        public bool Pack { get; }

        /// <summary>
        /// List of files that will be packed into the resulting FMPA file
        /// </summary>
        public IReadOnlyList<FileInfo> FilesToPack { get; }

        internal Exporter(in Package package, in bool pack, in IReadOnlyList<FileInfo> filesToPack)
            => (Package, Pack, FilesToPack) = (package, pack, filesToPack);

        private void ExportPackage(string path, Formatting formatting, JsonSerializerSettings? settings)
        {
            string json = JsonConvert.SerializeObject(Package, formatting, settings);
            File.WriteAllText(path, json);
        }

        private async Task ExportPackageAsync(string path, Formatting formatting, JsonSerializerSettings? settings)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                                              4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

            string json = JsonConvert.SerializeObject(Package, formatting, settings);
            var buffer = Encoding.UTF8.GetBytes(json);

            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private void ExportArchive(FileInfo file, Formatting formatting, JsonSerializerSettings? settings)
        {
            if (!file.Directory.Exists) file.Directory.Create();

            using var stream = file.Open(FileMode.Create, FileAccess.Write);
            // We can choose best speed here because the only thing we're compressing is other zip archives and some JSON
            var options = new ZipWriterOptions(CompressionType.Deflate) { DeflateCompressionLevel = CompressionLevel.BestSpeed };
            using var writer = new ZipWriter(stream, options);

            string json = JsonConvert.SerializeObject(Package, formatting, settings);
            byte[] data = Encoding.UTF8.GetBytes(json);
            using var jsonStream = new MemoryStream(data);
            writer.Write("pack.json", jsonStream);

            foreach (var f in FilesToPack)
                writer.Write(f.Name, f);
        }

        private Task ExportArchiveAsync(FileInfo file, Formatting formatting, JsonSerializerSettings? settings)
            => Task.Run(() => ExportArchive(file, formatting, settings));

        /// <summary>
        /// Exports the package to the specified file
        /// </summary>
        public void Export(string path, Formatting formatting = Formatting.Indented, in JsonSerializerSettings? settings = null)
        {
            if (Pack) ExportArchive(new FileInfo(path), formatting, settings);
            else ExportPackage(path, formatting, settings);
        }

        /// <summary>
        /// Exports the package to the specified file
        /// </summary>
        public void Export(FileInfo file, Formatting formatting = Formatting.Indented, in JsonSerializerSettings? settings = null)
        {
            if (Pack) ExportArchive(file, formatting, settings);
            else ExportPackage(file.FullName, formatting, settings);
        }

        /// <summary>
        /// Exports the package to the specified file
        /// </summary>
        public Task ExportAsync(string path, Formatting formatting = Formatting.Indented, in JsonSerializerSettings? settings = null)
        {
            if (Pack) return ExportArchiveAsync(new FileInfo(path), formatting, settings);
            else return ExportPackageAsync(path, formatting, settings);
        }

        /// <summary>
        /// Exports the package to the specified file
        /// </summary>
        public Task ExportAsync(FileInfo file, Formatting formatting = Formatting.Indented, in JsonSerializerSettings? settings = null)
        {
            if (Pack) return ExportArchiveAsync(file, formatting, settings);
            else return ExportPackageAsync(file.FullName, formatting, settings);
        }
    }
}
