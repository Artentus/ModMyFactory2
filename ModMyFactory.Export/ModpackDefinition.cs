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
    public sealed class ModpackDefinition
    {
        public int Uid { get; }

        public string Name { get; }

        public IReadOnlyList<int> ModIds { get; }

        public IReadOnlyList<int> ModpackIds { get; }

        /// <param name="uid">Unique ID of the modpack inside the package</param>
        /// <param name="name">Name of the modpack</param>
        /// <param name="modIds">IDs of mods in the modpack</param>
        /// <param name="modpackIds">IDs of other modpacks inside the modpack</param>
        [JsonConstructor]
        public ModpackDefinition(in int uid, in string name, in IReadOnlyList<int> modIds, in IReadOnlyList<int> modpackIds)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (modIds is null) throw new ArgumentNullException(nameof(modIds));
            if (modpackIds is null) throw new ArgumentNullException(nameof(modpackIds));

            Uid = uid;
            Name = name;
            ModIds = modIds;
            ModpackIds = modpackIds;
        }
    }
}
