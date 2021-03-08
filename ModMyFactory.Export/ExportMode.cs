//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.Export
{
    [Flags]
    public enum ExportMode
    {
        /// <summary>
        /// Value mask
        /// </summary>
        Mask = 0x0000FFFF,

        /// <summary>
        /// The mod is included in the pack
        /// </summary>
        Included = 0x00010000,

        /// <summary>
        /// Download a newer version if available even if the mod is included
        /// </summary>
        DownloadNewer = 0x00020000,

        //------------------------------------------------------------------------------------------------

        /// <summary>
        /// Invalid mode
        /// </summary>
        [Obsolete]
        Invalid = 0,

        /// <summary>
        /// Download the latest available version
        /// </summary>
        LatestVersion = 1,

        /// <summary>
        /// Download a specific version
        /// </summary>
        SpecificVersion = 2,

        /// <summary>
        /// Download the latest available version that is compatible with a specific version of Factorio
        /// </summary>
        FactorioVersion = 3
    }
}
