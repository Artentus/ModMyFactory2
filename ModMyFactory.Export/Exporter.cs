//  Copyright (C) 2020 Mathis Rech
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

        private void ExportPackage(string path)
        {
            string json = JsonConvert.SerializeObject(Package, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private async Task ExportPackageAsync(string path)
        {
            string json = JsonConvert.SerializeObject(Package, Formatting.Indented);
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                                              4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            var buffer = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private void ExportArchive(FileInfo file)
        {
            using var stream = file.Open(FileMode.Create, FileAccess.Write);
            // We can choose best speed here because the only thing we're compressing is other zip archives and some JSON
            var options = new ZipWriterOptions(CompressionType.Deflate) { DeflateCompressionLevel = CompressionLevel.BestSpeed };
            using var writer = new ZipWriter(stream, options);

            string json = JsonConvert.SerializeObject(Package, Formatting.Indented);
            byte[] data = Encoding.UTF8.GetBytes(json);
            using var jsonStream = new MemoryStream(data);
            writer.Write("pack.json", jsonStream);

            foreach (var f in FilesToPack)
                writer.Write(f.Name, f);
        }

        private Task ExportArchiveAsync(FileInfo file)
            => Task.Run(() => ExportArchive(file));

        /// <summary>
        /// Exports the package to the specified file
        /// </summary>
        public void Export(string path)
        {
            if (Pack) ExportArchive(new FileInfo(path));
            else ExportPackage(path);
        }

        /// <summary>
        /// Exports the package to the specified file
        /// </summary>
        public void Export(FileInfo file)
        {
            if (Pack) ExportArchive(file);
            else ExportPackage(file.FullName);
        }

        /// <summary>
        /// Exports the package to the specified file
        /// </summary>
        public Task ExportAsync(string path)
        {
            if (Pack) return ExportArchiveAsync(new FileInfo(path));
            else return ExportPackageAsync(path);
        }

        /// <summary>
        /// Exports the package to the specified file
        /// </summary>
        public Task ExportAsync(FileInfo file)
        {
            if (Pack) return ExportArchiveAsync(file);
            else return ExportPackageAsync(file.FullName);
        }
    }
}
