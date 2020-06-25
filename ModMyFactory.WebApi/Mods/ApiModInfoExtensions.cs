//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using System.Linq;

namespace ModMyFactory.WebApi.Mods
{
    public static class ApiModInfoExtensions
    {
        /// <summary>
        /// Gets the latest release of the mod and properly resolves extended info
        /// </summary>
        public static ModReleaseInfo GetLatestReleaseSafe(this ApiModInfo info)
        {
            if (info.LatestRelease.HasValue) return info.LatestRelease.Value;

            ModReleaseInfo max = default;
            foreach (var release in info.Releases)
            {
                if (release.Version > max.Version)
                    max = release;
            }
            return max;
        }

        /// <summary>
        /// Gets the latest release of the mod that is compatible with a given version of Factorio
        /// </summary>
        public static ModReleaseInfo? GetLatestRelease(this ApiModInfo info, AccurateVersion factorioVersion)
        {
            ModReleaseInfo? max = null;
            foreach (var release in info.Releases.Where(r => r.Info.FactorioVersion == factorioVersion))
            {
                if ((max is null) || (release.Version > max.Value.Version))
                    max = release;
            }
            return max;
        }
    }
}
