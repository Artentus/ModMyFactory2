//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.WebApi.Factorio
{
    /// <summary>
    /// Specifies the build of Factorio.
    /// </summary>
    public enum FactorioBuild
    {
        /// <summary>
        /// The main release build of Factorio.
        /// </summary>
        Alpha,

        /// <summary>
        /// The demo build of Factorio.
        /// </summary>
        Demo,

        /// <summary>
        /// A build of Factorio with no graphical user interface intended for use on servers.
        /// </summary>
        Headless
    }
}
