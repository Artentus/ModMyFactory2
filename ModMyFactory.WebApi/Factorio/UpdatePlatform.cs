//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.WebApi.Factorio
{
    /// <summary>
    /// Specifies a target platform for updating.
    /// </summary>
    public enum UpdatePlatform
    {
        /// <summary>
        /// Windows 64 bit.
        /// </summary>
        Win64,

        /// <summary>
        /// Mac OS.
        /// </summary>
        Mac,

        /// <summary>
        /// Linux 64 bit.
        /// </summary>
        Linux64,

        /// <summary>
        /// Linux 64 bit headles.
        /// </summary>
        Headless64
    }
}
