//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using Newtonsoft.Json;
using System;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// Stores information about the state of a mod family
    /// </summary>
    public sealed class ModFamilyStateInfo
    {
        /// <summary>
        /// A representation of the base mod
        /// </summary>
        public static readonly ModFamilyStateInfo Base = new ModFamilyStateInfo("base", true, default);

        /// <summary>
        /// Name of the family
        /// </summary>
        [JsonProperty("name")]
        public string FamilyName { get; }

        /// <summary>
        /// Whether a mod in this family is enabled
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; }

        /// <summary>
        /// The version of the enabled mod in the family. Undefined if no mod is enabled
        /// </summary>
        [JsonProperty("version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AccurateVersion Version { get; }

        [JsonConstructor]
        private ModFamilyStateInfo(string name, bool enabled, AccurateVersion version)
            => (FamilyName, Enabled, Version) = (name, enabled, version);

        /// <summary>
        /// Creates state information for a mod family
        /// </summary>
        public static ModFamilyStateInfo FromFamily(ModFamily family)
        {
            var name = family.FamilyName;
            bool enabled = false;
            AccurateVersion version = default;

            var enabledMod = family.EnabledMod;
            if (enabledMod != null)
            {
                enabled = true;
                var defaultMod = family.GetDefaultMod();
                if (enabledMod != defaultMod) version = enabledMod.Version; // Only store version if enabled mod is not default
            }

            return new ModFamilyStateInfo(name, enabled, version);
        }

        /// <summary>
        /// Applies state information to a mod family
        /// </summary>
        public void ApplyToFamily(in ModFamily family)
        {
            if (family is null)
                throw new ArgumentNullException(nameof(family));
            if (family.FamilyName != FamilyName)
                throw new ArgumentException("Family is not compatible to this state", nameof(family));

            if (family.Count > 0)
            {
                if (Enabled)
                {
                    if (Version == default)
                    {
                        family.GetDefaultMod().Enabled = true;
                    }
                    else
                    {
                        if (family.Contains(Version, out var mod)) mod.Enabled = true;
                        else family.GetDefaultMod().Enabled = true;
                    }
                }
                else
                {
                    foreach (var mod in family)
                        mod.Enabled = false;
                }
            }
        }
    }
}
