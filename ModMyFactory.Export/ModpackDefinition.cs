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
    public sealed class ModpackDefinition
    {
        static volatile int GlobalUid = 0;


        public string Name { get; }

        [JsonConstructor]
        private ModpackDefinition(int uid, string name, ModDefinition[] mods, string[] modpacks, int[] modIds, int[] modpackIds)
        {
            Uid = uid;
            Name = name;
            Mods = mods;
            Modpacks = modpacks;
            ModIds = modIds;
            ModpackIds = modpackIds;
        }

        // -------------- File version 1 --------------

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ModDefinition[] Mods { get; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Modpacks { get; }

        public ModpackDefinition(string name, ModDefinition[] mods, string[] modpacks)
        {
            Uid = -1;
            Name = name;
            Mods = mods;
            Modpacks = modpacks;
        }

        // -------------- File version 2 --------------

        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Uid { get; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int[] ModIds { get; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int[] ModpackIds { get; }

        public ModpackDefinition(string name, int[] modIds, int[] modpackIds)
        {
            Uid = GlobalUid;
            GlobalUid++;

            Name = name;
            ModIds = modIds;
            ModpackIds = modpackIds;
        }
    }
}
