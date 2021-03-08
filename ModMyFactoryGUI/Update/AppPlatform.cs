//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactoryGUI.Update
{
    enum AppPlatform
    {
        /// <summary>
        /// Linux x64 build, running on .Net Core and all Linux Debian and compatible distibutions
        /// </summary>
        Linux64,

        /// <summary>
        /// Mac x64 build, running on .Net Core and macOS 10.13 High Sierra or higher
        /// </summary>
        OsX,

        /// <summary>
        /// Windows AnyCPU build, running on .Net Framework (only x64 and Windows 7 or higher officially supported)
        /// </summary>
        Windows,

        /// <summary>
        /// Universal AnyCPU build, running on .Net Core and any operating system supporting .Net Core 3.0 (only x64 officially supported)
        /// </summary>
        Universal,
    }

    static class AppPlatformExtensions
    {
        public static string AssetSuffix(this AppPlatform value)
        {
            return value switch
            {
                AppPlatform.Linux64 => "linux64",
                AppPlatform.OsX => "osx",
                AppPlatform.Windows => "win64",
                AppPlatform.Universal => "universal",
                _ => throw new ArgumentException("Unknown platform", nameof(value)),
            };
        }
    }
}
