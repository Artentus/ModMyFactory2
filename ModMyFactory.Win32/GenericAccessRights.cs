//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.Win32
{
    /// <summary>
    /// You can use generic access rights to specify the type of access you need when you are opening a handle to an object.
    /// This is typically simpler than specifying all the corresponding standard and specific rights.
    /// </summary>
    [Flags]
    public enum GenericAccessRights : uint
    {
        /// <summary>
        /// Read access.
        /// </summary>
        Read = 0x80000000,

        /// <summary>
        /// Write access.
        /// </summary>
        Write = 0x40000000,

        /// <summary>
        /// Execute access.
        /// </summary>
        Execute = 0x20000000,

        /// <summary>
        /// All possible access rights.
        /// </summary>
        All = 0x10000000,
    }
}
