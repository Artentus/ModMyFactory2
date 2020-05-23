//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Export;
using ModMyFactory.Mods;
using ModMyFactoryGUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal sealed class ImportHelper
    {
        private readonly IEnumerable<string> _paths;

        public ImportHelper(IEnumerable<string> paths)
            => _paths = paths;

        private static string GetOriginalFileName(FileInfo file)
        {
            int index = file.Name.IndexOf('+');
            if (index < 1) return file.Name;

            string originalName = file.Name.Substring(index + 1);
            if (string.IsNullOrEmpty(originalName)) return file.Name;
            return originalName;
        }

        private static Task<AccurateVersion> GetVersionToDownloadAsync(ModDefinition modDef)
        {
            return modDef.MaskedExportMode switch
            {
                ExportMode.LatestVersion => modDef.GetLatestVersionAsync(),
                ExportMode.FactorioVersion => modDef.GetLatestVersionAsync(modDef.FactorioVersion),
                ExportMode.SpecificVersion => Task.FromResult(modDef.Version),
                _ => throw new InvalidOperationException("Invalid export mode")
            };
        }

        private async Task<Mod> ImportModAsync(ImportResult result, ModDefinition modDef, AccurateVersion versionToDownload, IProgress<double> progress)
        {
            IModFile modFile;
            if (result.TryGetExtractedFile(modDef, out var file))
            {
                // We need to restore the original file name (without the ID) or it will not be recognized as a valid mod file
                var originalName = GetOriginalFileName(file);
                file.Rename(originalName);

                bool success;
                (success, modFile) = await ModFile.TryLoadAsync(file);
                if (success)
                {
                    if ((modFile.Info.Version < versionToDownload) && modDef.DownloadNewer)
                    {
                        // File was in the package but we have to download a newer version
                        modFile.Delete();
                        modFile = await modDef.DownloadAsync(versionToDownload, progress);
                    }
                    else
                    {
                        // We can use the file inside the package
                        var modDir = Program.Locations.GetModDir(modFile.Info.FactorioVersion);
                        await modFile.MoveToAsync(modDir.FullName);
                    }
                }
                else
                {
                    // Mod file in the package was invalid
                    modFile = await modDef.DownloadAsync(versionToDownload, progress);
                }
            }
            else
            {
                // Package did not contain the mod file
                modFile = await modDef.DownloadAsync(versionToDownload, progress);
            }

            progress.Report(1);

            if (modFile is null)
            {
                // Download was unsuccessfull
                // ToDo: ask for retry
                return null;
            }
            else
            {
                var mod = new Mod(modFile);
                Program.Manager.AddMod(mod);
                return mod;
            }
        }

        private async Task<Dictionary<int, Mod>> ImportModsAsync(ImportResult result, IProgress<(string, double)> progress)
        {
            var modMappings = new Dictionary<int, Mod>(); // Lookup table for imported mods
            foreach (var modDef in result.Package.Mods)
            {
                progress.Report((modDef.Name, 0));

                var versionToDownload = await GetVersionToDownloadAsync(modDef);
                if (!Program.Manager.ContainsMod(modDef.Name, versionToDownload, out var mod))
                {
                    // Required mod is not locally available, we have to import it
                    var p = new Progress<double>(val => progress.Report((modDef.Name, val)));
                    mod = await ImportModAsync(result, modDef, versionToDownload, p);
                }

                progress.Report((modDef.Name, 1));
                if (!(mod is null)) modMappings.Add(modDef.Uid, mod);
            }

            return modMappings;
        }

        private void ImportModpacks(ImportResult result, Dictionary<int, Mod> modMappings)
        {
            var modpackMappings = new Dictionary<int, Modpack>(); // Lookup table for imported modpacks
            foreach (var packDef in result.Package.Modpacks)
            {
                var modpack = Program.CreateModpack();
                modpack.DisplayName = packDef.Name;

                foreach (int modId in packDef.ModIds)
                {
                    if (modMappings.TryGetValue(modId, out var mod))
                        modpack.Add(mod);
                }
                foreach (int packId in packDef.ModpackIds)
                {
                    if (modpackMappings.TryGetValue(packId, out var subPack))
                        modpack.Add(subPack);
                }

                modpackMappings.Add(packDef.Uid, modpack);
            }
        }

        private async Task ImportPackageAsync(FileInfo packageFile)
        {
            var result = await Importer.ImportAsync(packageFile, Program.TemporaryDirectory.FullName);

            try
            {
                var dialog = new ProgressDialog { Minimum = 0, Maximum = 1 };
                var progress = new Progress<(string, double)>(t =>
                {
                    dialog.Description = t.Item1;
                    dialog.Value = t.Item2;
                });

                var dialogTask = dialog.ShowDialog(App.Current.MainWindow);

                var modMappings = await ImportModsAsync(result, progress);

                dialog.IsIndeterminate = true;
                dialog.Description = string.Empty;

                ImportModpacks(result, modMappings);

                dialog.Close();
                await dialogTask;
            }
            finally
            {
                result.CleanupFiles();
            }
        }

        public async Task ImportPackagesAsync()
        {
            foreach (var path in _paths)
            {
                var packageFile = new FileInfo(path);
                if (packageFile.Exists) await ImportPackageAsync(packageFile);
            }
        }
    }
}
