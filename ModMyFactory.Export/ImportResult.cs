//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModMyFactory.Export
{
    /// <summary>
    /// The result of an import
    /// </summary>
    public sealed class ImportResult
    {
        /// <summary>
        /// The package that was imported
        /// </summary>
        public Package Package { get; }

        /// <summary>
        /// Optional files that were extracted from the package during import
        /// </summary>
        public IReadOnlyList<FileInfo> ExtractedFiles { get; }

        internal ImportResult(Package package)
            => (Package, ExtractedFiles) = (package, new FileInfo[0]);

        internal ImportResult(Package package, IReadOnlyList<FileInfo> extractedFiles)
            => (Package, ExtractedFiles) = (package, extractedFiles);

        /// <summary>
        /// Tries to get the extracted file corresponding to a mod definition in the imported package
        /// </summary>
        public bool TryGetExtractedFile(ModDefinition mod, out FileInfo file)
        {
            file = null;
            if (!mod.Included) return false;
            if (!Package.Mods.Contains(mod)) return false;
            if (ExtractedFiles is null) return false;

            string prefix = $"{mod.Uid}+";
            foreach (var extFile in ExtractedFiles)
            {
                if (extFile.Name.StartsWith(prefix))
                {
                    file = extFile;
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Deletes all extracted files from the hard drive
        /// </summary>
        public void CleanupFiles()
        {
            if (!(ExtractedFiles is null))
            {
                foreach (var file in ExtractedFiles)
                {
                    if (file.Exists) file.Delete();
                }
            }
        }
    }
}
