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
using ModMyFactoryGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal sealed class ExportHelper
    {
        // Helper struct to hold temporary information about modpacks
        private struct ModpackDefinitionTemplate
        {
            public int Uid;
            public string Name;
            public IReadOnlyList<int> ModIds;

            public ModpackDefinition CreateDefinition(IReadOnlyList<int> packIds)
                => new ModpackDefinition(Uid, Name, ModIds, packIds);
        }

        // Helper class to uniquely differentiate between different export modes for the same mod
        private sealed class ModDefinitionIdentifier : IEquatable<ModDefinitionIdentifier>
        {
            public string ModName { get; }

            public ExportMode ExportMode { get; }

            public AccurateVersion Version { get; }

            public ModDefinitionIdentifier(string modName, ExportMode exportMode, AccurateVersion version)
                => (ModName, ExportMode, Version) = (modName, exportMode, version);

            public static bool operator ==(ModDefinitionIdentifier first, ModDefinitionIdentifier second)
            {
                if (first is null) return (second is null);
                return first.Equals(second);
            }

            public static bool operator !=(ModDefinitionIdentifier first, ModDefinitionIdentifier second)
            {
                if (first is null) return !(second is null);
                return !first.Equals(second);
            }

            public bool Equals(ModDefinitionIdentifier? other)
            {
                if (other is null) return false;

                return ModName.Equals(other.ModName, StringComparison.InvariantCulture)
                    && ExportMode == other.ExportMode
                    && Version == other.Version;
            }

            public override bool Equals(object? obj)
            {
                if (obj is ModDefinitionIdentifier other)
                    return Equals(other);

                return false;
            }

            public override int GetHashCode()
                => ModName.GetHashCode() ^ ExportMode.GetHashCode() ^ Version.GetHashCode();
        }

        private readonly ICollection<ModpackExportViewModel> _modpacksToExport;
        private readonly Dictionary<ModDefinitionIdentifier, ModDefinition> _modMappings
            = new Dictionary<ModDefinitionIdentifier, ModDefinition>();
        private int _modId;

        public ExportHelper(ICollection<ModpackExportViewModel> modpacksToExport)
            => _modpacksToExport = modpacksToExport;

        private static AccurateVersion GetExportVersion(ModExportViewModel vm, ExportMode mode)
        {
            var maskedMode = mode & ExportMode.Mask;
            return maskedMode switch
            {
                ExportMode.LatestVersion => default,
                ExportMode.FactorioVersion => vm.Mod.FactorioVersion,
                ExportMode.SpecificVersion => vm.Mod.Version,
                _ => throw new InvalidOperationException("Invalid export mode")
            };
        }

        private static async Task<FileInfo?> GetExportFileAsync(ModExportViewModel vm, int id)
        {
            var modFile = vm.Mod.File;
            if (modFile is null) return null;

            // If the mod is extracted we have to re-pack it first
            if (modFile.IsExtracted)
            {
                var extracted = (ExtractedModFile)modFile;
                modFile = await extracted.PackAsync(Program.TemporaryDirectory.FullName);
            }
            else
            {
                modFile = await modFile.CopyToAsync(Program.TemporaryDirectory.FullName);
            }

            var file = new FileInfo(modFile.FilePath);
            file.Rename($"{id}+{file.Name}"); // Append ID to filename
            return file;
        }

        private async Task<IReadOnlyList<int>> BuildModDefinitionsAsync(ModpackExportViewModel vm, ExporterFactory factory)
        {
            var result = new List<int>(vm.Mods.Count);

            foreach (var modVm in vm.Mods)
            {
                var mode = modVm.GetExportMode();
                var version = GetExportVersion(modVm, mode);
                var identifier = new ModDefinitionIdentifier(modVm.Mod.Name, mode, version);

                // We use the unique identifier object to check whether a definition already exists
                if (!_modMappings.TryGetValue(identifier, out var modDef))
                {
                    // No definition found, create one
                    modDef = new ModDefinition(_modId, modVm.Mod.Name, mode, version);
                    _modMappings.Add(identifier, modDef);

                    // Include file if we need to
                    if (mode.HasFlag(ExportMode.Included))
                    {
                        var file = await GetExportFileAsync(modVm, _modId);
                        if (!(file is null)) factory.FilesToPack.Add(file);
                    }

                    _modId++;
                }

                // Add ID of the definition (found or created) to the reference list
                result.Add(modDef.Uid);
            }

            return result;
        }

        private async Task<IReadOnlyDictionary<Modpack, ModpackDefinitionTemplate>> BuildModpackDefinitionTemplatesAsync(ICollection<ModpackExportViewModel> modpacks, ExporterFactory factory)
        {
            int modpackId = 0;
            var templates = new Dictionary<Modpack, ModpackDefinitionTemplate>(modpacks.Count);

            foreach (var vm in modpacks)
            {
                var modIds = await BuildModDefinitionsAsync(vm, factory); // This also builds all required mod definitions

                // We assiciate every modpack with its respective template so we can find the templates easier later on
                var template = new ModpackDefinitionTemplate { Uid = modpackId, Name = vm.DisplayName, ModIds = modIds };
                templates.Add(vm.Modpack, template);

                modpackId++;
            }

            return templates;
        }

        private static IReadOnlyList<int> GetPackReferenceIds(Modpack pack, IReadOnlyDictionary<Modpack, ModpackDefinitionTemplate> templates)
        {
            var ids = new List<int>();

            foreach (var subPack in pack.Modpacks)
            {
                var template = templates[subPack];
                ids.Add(template.Uid);
            }

            return ids;
        }

        public async Task<ExporterFactory> PrepareExportAsync()
        {
            // Reset state
            _modId = 0;

            var factory = new ExporterFactory();

            // We build templates for all modpacks to generate unique IDs for mods and modpacks
            var templates = await BuildModpackDefinitionTemplatesAsync(_modpacksToExport, factory);

            // Definitions for all mods have been built already
            factory.ModDefinitions.AddRange(_modMappings.Values);

            foreach (var pack in _modpacksToExport.Select(p => p.Modpack))
            {
                var packIds = GetPackReferenceIds(pack, templates);
                var template = templates[pack];
                factory.ModpackDefinitions.Add(template.CreateDefinition(packIds));
            }

            return factory;
        }
    }
}
