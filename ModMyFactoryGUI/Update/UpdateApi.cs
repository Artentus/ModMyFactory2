//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Octokit;

namespace ModMyFactoryGUI.Update
{
    static class UpdateApi
    {
        private static (Release?, TagVersion?) GetLatestRelease(IReadOnlyList<Release> releases, bool includePrereleases)
        {
            Release? latest = null;
            TagVersion? version = null;

            foreach (var release in releases)
            {
                if (!release.Prerelease || includePrereleases)
                {
                    var v = TagVersion.Parse(release.TagName);

                    if ((version is null) || (v > version))
                    {
                        latest = release;
                        version = v;
                    }
                }
            }

            return (latest, version);
        }

        private static bool TryGetAsset(in Release? release, [NotNullWhen(true)] out ReleaseAsset? result)
        {
            result = null;
            if (release is null) return false;

            string name = $"{release.TagName}_{VersionStatistics.AppPlatform.AssetSuffix()}.zip";
            foreach (var asset in release.Assets)
            {
                if (string.Equals(asset.Name, name, StringComparison.InvariantCulture))
                {
                    result = asset;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public static async Task<(bool, TagVersion?, string?)> CheckForUpdateAsync(bool includePrereleases)
        {
            var client = new ReleasesClient();
            var (success, releases) = await client.TryRequestReleasesAsync();
            if (!success) return (false, null, null);

            var (latest, version) = GetLatestRelease(releases!, includePrereleases);
            if (version > VersionStatistics.AppVersion)
            {
                if (TryGetAsset(latest, out var asset)) return (true, version, asset.BrowserDownloadUrl);
                else return (false, null, null);
            }
            else
            {
                return (false, null, null);
            }
        }

        public static async Task<string> RequestChangelogAsync()
        {
            const string url = "https://raw.githubusercontent.com/Artentus/ModMyFactory2/master/Changelog.md";
            using var wc = new WebClient();
            return await wc.DownloadStringTaskAsync(url);
        }
    }
}
