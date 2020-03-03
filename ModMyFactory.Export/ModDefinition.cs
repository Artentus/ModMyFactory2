//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using Newtonsoft.Json;
using System.ComponentModel;

namespace ModMyFactory.Export
{
    public sealed class ModDefinition
    {
        static volatile int GlobalUid = 0;


        public string Name { get; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AccurateVersion Version { get; }

        [JsonConstructor]
        private ModDefinition(int uid, string name, ExportMode exportMode, AccurateVersion version, AccurateVersion factorioVersion)
        {
            Uid = uid;
            Name = name;
            ExportMode = exportMode;
            Version = version;
            FactorioVersion = factorioVersion;
        }

        // -------------- File version 1 --------------

        public ModDefinition(string name, AccurateVersion version = default)
        {
            Uid = -1;
            Name = name;
            Version = version;
        }

        // -------------- File version 2 --------------

        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Uid { get; }

        [DefaultValue(ExportMode.Version1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ExportMode ExportMode { get; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AccurateVersion FactorioVersion { get; }

        [JsonIgnore]
        public bool Included => ExportMode.HasFlag(ExportMode.Included);

        [JsonIgnore]
        public bool DownloadNewer => ExportMode.HasFlag(ExportMode.DownloadNewer);

        [JsonIgnore]
        public ExportMode MaskedExportMode => ExportMode & ExportMode.Mask;

        public ModDefinition(string name, ExportMode exportMode, AccurateVersion versionOrFactorioVersion = default)
        {
            Uid = GlobalUid;
            GlobalUid++;

            Name = name;
            ExportMode = exportMode;

            exportMode &= ExportMode.Mask;
            if (exportMode == ExportMode.SpecificVersion)
                Version = versionOrFactorioVersion;
            else if (exportMode == ExportMode.FactorioVersion)
                FactorioVersion = versionOrFactorioVersion;
        }
    }
}
