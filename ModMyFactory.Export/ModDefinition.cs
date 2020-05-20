//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using Newtonsoft.Json;
using System;

namespace ModMyFactory.Export
{
    public sealed class ModDefinition : IEquatable<ModDefinition>
    {
        public int Uid { get; }

        public string Name { get; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AccurateVersion Version { get; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AccurateVersion FactorioVersion { get; }

        public ExportMode ExportMode { get; }

        [JsonIgnore] public ExportMode MaskedExportMode => ExportMode & ExportMode.Mask;
        [JsonIgnore] public bool Included => ExportMode.HasFlag(ExportMode.Included);
        [JsonIgnore] public bool DownloadNewer => ExportMode.HasFlag(ExportMode.DownloadNewer);

        [JsonConstructor]
        private ModDefinition(in int uid, in string name, in ExportMode exportMode, in AccurateVersion version, in AccurateVersion factorioVersion)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            Uid = uid;
            Name = name;
            ExportMode = exportMode;
            Version = version;
            FactorioVersion = factorioVersion;

#pragma warning disable CS0612
            if (MaskedExportMode == ExportMode.Invalid) throw new ArgumentException("Export mode is not valid", nameof(exportMode));
#pragma warning restore CS0612
        }

        /// <param name="uid">Unique ID of the mod inside the package</param>
        /// <param name="name">Name of the mod</param>
        /// <param name="exportMode">Export mode of the mod</param>
        /// <param name="version">Either the mods version, the target Factorio version or ignored depending on export mode</param>
        public ModDefinition(in int uid, in string name, in ExportMode exportMode, in AccurateVersion version = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            Uid = uid;
            Name = name;
            ExportMode = exportMode;

            var masked = MaskedExportMode;
            switch (masked)
            {
                case ExportMode.LatestVersion:
                    // We don't need to store any version information
                    Version = default;
                    FactorioVersion = default;
                    break;

                case ExportMode.SpecificVersion:
                    Version = version;
                    FactorioVersion = default;
                    break;

                case ExportMode.FactorioVersion:
                    Version = default;
                    FactorioVersion = version;
                    break;
            }

#pragma warning disable CS0612
            if (masked == ExportMode.Invalid) throw new ArgumentException("Export mode is not valid", nameof(exportMode));
#pragma warning restore CS0612
        }

        public bool Equals(ModDefinition other)
        {
            if (other is null) return false;
            return (Uid == other.Uid) && (Name == other.Name) && (Version == other.Version) && (FactorioVersion == other.FactorioVersion);
        }

        public override bool Equals(object obj)
        {
            if (obj is ModDefinition other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
            => Uid.GetHashCode() ^ Name.GetHashCode() ^ Version.GetHashCode() ^ FactorioVersion.GetHashCode();

        public static bool operator ==(ModDefinition first, ModDefinition second)
        {
            if (first is null) return second is null;
            return first.Equals(second);
        }

        public static bool operator !=(ModDefinition first, ModDefinition second)
        {
            if (first is null) return !(second is null);
            return !first.Equals(second);
        }
    }
}
