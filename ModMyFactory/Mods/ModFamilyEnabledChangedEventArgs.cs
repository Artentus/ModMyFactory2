//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// An event that occurs if the endabled states of mods in a family change
    /// </summary>
    public class ModFamilyEnabledChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The family that raised the event originally
        /// </summary>
        public ModFamily Family { get; }

        public ModFamilyEnabledChangedEventArgs(ModFamily family)
        {
            Family = family;
        }
    }
}
