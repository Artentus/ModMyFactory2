//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.Export;
using ModMyFactory.Mods;
using ModMyFactory.WebApi.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal static class ModDefinitionExtensions
    {
        private static readonly Dictionary<string, ApiModInfo> _cache = new Dictionary<string, ApiModInfo>();

        private static Task<ApiModInfo> GetInfoAsync(string modName)
        {
            if (_cache.TryGetValue(modName, out var info))
                return Task.FromResult(info);

            return ModApi.RequestModInfoAsync(modName);
        }

        private static async Task<ModReleaseInfo?> GetReleaseAsync(this ModDefinition modDef, AccurateVersion version)
        {
            var info = await GetInfoAsync(modDef.Name);

            foreach (var release in info.Releases)
            {
                if (release.Version == version)
                    return release;
            }

            return null;
        }

        public static async Task<AccurateVersion> GetLatestVersionAsync(this ModDefinition modDef)
        {
            var info = await GetInfoAsync(modDef.Name);
            return info.GetLatestReleaseSafe().Version;
        }

        public static async Task<AccurateVersion> GetLatestVersionAsync(this ModDefinition modDef, AccurateVersion factorioVersion)
        {
            var info = await GetInfoAsync(modDef.Name);

            ModReleaseInfo max = default;
            foreach (var release in info.Releases.Where(r => r.Info.FactorioVersion == factorioVersion))
            {
                if (release.Version > max.Version)
                    max = release;
            }
            return max.Version;
        }

        public static async Task<IModFile> DownloadAsync(this ModDefinition modDef, AccurateVersion version, IProgress<double> progress)
        {
            // We do not need to download using the queue because while importing we block the app and display progress separately

            var result = await modDef.GetReleaseAsync(version);
            if (result.HasValue)
            {
                var release = result.Value;

                var (isLoggedIn, username, token) = await App.Current.Credentials.TryLogInAsync();
                if (isLoggedIn.IsTrue())
                {
                    var dir = Program.Locations.GetModDir(release.Info.FactorioVersion);
                    string fileName = Path.Combine(dir.FullName, release.FileName);

                    var file = await ModApi.DownloadModReleaseAsync(release, username, token, fileName);
                    var (success, modFile) = await ModFile.TryLoadAsync(file);
                    if (success) return modFile;
                }
            }

            return null;
        }
    }
}
