//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Collections.Generic;
using System.IO;

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
    }
}
