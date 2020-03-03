//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System.ComponentModel;

namespace ModMyFactory.Export
{
    public sealed class Package
    {
        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Version { get; }

        public ModDefinition[] Mods { get; }

        public ModpackDefinition[] Modpacks { get; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IncludesVersionInfo { get; }

        [JsonConstructor]
        private Package(int version, ModDefinition[] mods, ModpackDefinition[] modpacks, bool includesVersionInfo)
        {
            Version = version;
            Mods = mods;
            Modpacks = modpacks;
            IncludesVersionInfo = includesVersionInfo;
        }

        // -------------- File version 1 --------------
        public Package(ModDefinition[] mods, ModpackDefinition[] modpacks, bool includesVersionInfo)
        {
            Version = 1;
            Mods = mods;
            Modpacks = modpacks;
            IncludesVersionInfo = includesVersionInfo;
        }

        // -------------- File version 2 --------------

        public Package(ModDefinition[] mods, ModpackDefinition[] modpacks)
        {
            Version = 2;
            Mods = mods;
            Modpacks = modpacks;
        }

        // -------------- File version 3 --------------

        // Mod settings
    }
}
