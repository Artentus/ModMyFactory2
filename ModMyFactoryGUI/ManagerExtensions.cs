//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Game;
using ModMyFactory.Mods;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    internal static class ManagerExtensions
    {
        /// <summary>
        /// Loads Factorio instances into the manager from the location specified by the location manager
        /// </summary>
        public static async Task LoadFactorioInstancesAsync(this Manager manager, LocationManager locations, SettingManager settings)
        {
            // Since we don't intrusively redirect any directories anymore like in MMF1 we can just always load the Steam instance
            var (success, instance) = await Factorio.TryLoadSteamAsync();
            if (success)
            {
                manager.AddInstance(instance);
                Log.Information($"Successfully loaded Factorio Steam instance");
            }

            // Load instances managed by the GUI
            var dir = locations.GetFactorioDir();
            foreach (var subDir in dir.EnumerateDirectories())
            {
                (success, instance) = await Factorio.TryLoadAsync(subDir);
                if (success)
                {
                    _ = manager.AddInstance(instance);
                    Log.Information($"Successfully loaded managed Factorio instance from '{subDir.Name}'");
                }
            }

            // Load external instances
            if (settings.TryGet(SettingName.ExternalInstances, out List<string> paths))
            {
                foreach (var path in paths)
                {
                    (success, instance) = await Factorio.TryLoadAsync(path);
                    if (success)
                    {
                        _ = manager.AddInstance(instance);
                        Log.Information($"Successfully loaded external Factorio instance from '{path}'");
                    }
                }
            }
        }

        /// <summary>
        /// Loads mods into the manager from the location specified by the location manager
        /// </summary>
        public static async Task LoadModsAsync(this Manager manager, LocationManager locations)
        {
            var dir = locations.GetModDir();
            foreach (var subDir in dir.EnumerateDirectories())
            {
                // Directory is only valid if its name is a major version
                if (AccurateVersion.TryParse(subDir.Name, out var version)
                    && (version.ToMajor() == version))
                {
                    var modManager = manager.GetModManager(version);

                    foreach (var fsi in subDir.EnumerateFileSystemInfos())
                    {
                        var (success, mod) = await Mod.TryLoadAsync(fsi);
                        if (success)
                        {
                            modManager.Add(mod);
                            Log.Information($"Successfully loaded mod {mod.Name} version {mod.Version}");
                        }
                    }
                }
            }
        }
    }
}
