//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.WebApi.Factorio
{
    /// <summary>
    /// Specifies a target platform.
    /// </summary>
    public enum Platform
    {
        /// <summary>
        /// Windows 64 bit automatic installer.
        /// </summary>
        Win64,

        /// <summary>
        /// Windows 64 bit ZIP package.
        /// </summary>
        Win64Manual,

        /// <summary>
        /// Mac OSX DMG package.
        /// </summary>
        OSX,

        /// <summary>
        /// Linux 64 bit tar.gz package.
        /// </summary>
        Linux64
    }
}
