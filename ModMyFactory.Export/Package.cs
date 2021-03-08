//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ModMyFactory.Export
{
    public sealed class Package
    {
        public int Version { get; }

        public IReadOnlyList<ModDefinition> Mods { get; }

        public IReadOnlyList<ModpackDefinition> Modpacks { get; }

        /// <param name="version">Version of the package</param>
        /// <param name="mods"></param>
        /// <param name="modpacks"></param>
        [JsonConstructor]
        internal Package(in int version, in IReadOnlyList<ModDefinition> mods, in IReadOnlyList<ModpackDefinition> modpacks)
        {
            if (version < 2) throw new ArgumentException("Only versions 2 or higher are supported", nameof(version));
            if (mods is null) throw new ArgumentNullException(nameof(mods));
            if (modpacks is null) throw new ArgumentNullException(nameof(modpacks));

            Version = version;
            Mods = mods;
            Modpacks = modpacks;
        }
    }
}
