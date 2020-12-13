//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.Mods
{
    public interface IModFile : IDisposable
    {
        /// <summary>
        /// The path of the mod file
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Information about the mod file
        /// </summary>
        ModInfo Info { get; }

        /// <summary>
        /// A stream containing bitmap data of the mods thumbnail<br/>
        /// May be null
        /// </summary>
        Stream? Thumbnail { get; }

        /// <summary>
        /// Determines whether this mod file is extracted
        /// </summary>
        bool IsExtracted { get; }

        /// <summary>
        /// Gets or sets if this mod file is enabled on file level<br/>
        /// A disabled mod file will not be recognized by Factorio
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Deletes this mod file
        /// </summary>
        void Delete();

        /// <summary>
        /// Copies this mod file to a new location
        /// </summary>
        /// <param name="destination">The path to copy this mod file to, excluding the mod files name itself</param>
        Task<IModFile> CopyToAsync(string destination);

        /// <summary>
        /// Moves this mod file to a new location
        /// </summary>
        /// <param name="destination">The path to move this mod file to, excluding the mod files name itself</param>
        Task MoveToAsync(string destination);
    }
}
