//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using System;
using System.Collections.Generic;

namespace ModMyFactory.WebApi.Factorio
{
    /// <summary>
    /// Contains information about Factorio releases.
    /// </summary>
    public class ReleaseInfo
    {
        private static readonly Dictionary<string, FactorioBuild> BuildVersions
            = new Dictionary<string, FactorioBuild>
            {
                { "alpha", FactorioBuild.Alpha },
                { "demo", FactorioBuild.Demo },
                { "headless", FactorioBuild.Headless }
            };

        private readonly Dictionary<FactorioBuild, AccurateVersion> internalDict;

        /// <summary>
        /// Gets the version of a specific build.
        /// </summary>
        public AccurateVersion this[FactorioBuild build]
        {
            get => internalDict[build];
            set => throw new NotSupportedException();
        }

        internal ReleaseInfo()
        {
            internalDict = new Dictionary<FactorioBuild, AccurateVersion>();
        }

        internal ReleaseInfo(IDictionary<string, string> dict)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));

            internalDict = new Dictionary<FactorioBuild, AccurateVersion>(dict.Count);
            foreach (var kvp in dict)
            {
                if (TryGetBuildFromString(kvp.Key, out var build) && AccurateVersion.TryParse(kvp.Value, out var version))
                    internalDict[build] = version;
            }
        }

        private static bool TryGetBuildFromString(string buildString, out FactorioBuild build)
        {
            return BuildVersions.TryGetValue(buildString.ToLowerInvariant(), out build);
        }

        /// <summary>
        /// Checks if there is version information on a specific build.
        /// </summary>
        public bool Contains(FactorioBuild build) => internalDict.ContainsKey(build);

        /// <summary>
        /// Tries to get version information on a specific build.
        /// </summary>
        public bool TryGetValue(FactorioBuild build, out AccurateVersion version) => internalDict.TryGetValue(build, out version);
    }
}
