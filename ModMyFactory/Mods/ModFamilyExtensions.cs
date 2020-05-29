//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.Mods
{
    public static class ModFamilyExtensions
    {
        /// <summary>
        /// Gets the default mod in this family<br/>
        /// The default mod is the mod that will be selected by Factorio if no version is specified
        /// </summary>
        public static Mod GetDefaultMod(this ModFamily family)
        {
            Mod max = null;
            foreach (var mod in family)
                if ((max is null) || (mod.Version > max.Version)) max = mod;
            return max;
        }
    }
}
