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
        public static bool TryGetLatestRelease(this ApiModInfo info, out ModReleaseInfo release)
        {
            if (info.LatestRelease.HasValue)
            {
                release = info.LatestRelease.Value;
                return true;
            }

            if (!(info.Releases is null) && (info.Releases.Length > 0))
            {
                ModReleaseInfo max = default;
                foreach (var r in info.Releases)
                {
                    if (r.Version > max.Version)
                        max = r;
                }
                release = max;
                return true;
            }

            release = default;
            return false;
        }

        /// <summary>
        /// Gets the latest release of the mod that is compatible with a given version of Factorio
        /// </summary>
        public static ModReleaseInfo? GetLatestRelease(this ApiModInfo info, AccurateVersion factorioVersion)
        {
            ModReleaseInfo? max = null;
            if (!(info.Releases is null))
            {
                foreach (var release in info.Releases.Where(r => r.Info.FactorioVersion == factorioVersion))
                {
                    if ((max is null) || (release.Version > max.Value.Version))
                        max = release;
                }
            }
            return max;
        }
    }
}
