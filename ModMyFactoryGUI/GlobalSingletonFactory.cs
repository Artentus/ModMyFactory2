//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    internal sealed class GlobalSingletonFactory
    {
        private readonly DirectoryInfo _binDir, _dataDir;

        public GlobalSingletonFactory(DirectoryInfo binDir, DirectoryInfo dataDir)
            => (_binDir, _dataDir) = (binDir, dataDir);

        public SettingManager LoadSettings()
        {
            var settingsFile = Path.Combine(_dataDir.FullName, "settings.json");
            return SettingManager.LoadSafe(settingsFile);
        }

        public async Task<(Manager, LocationManager)> CreateManagerAsync(SettingManager settings)
        {
            var manager = new Manager();
            var locations = new LocationManager(manager, settings, _binDir, _dataDir);

            await locations.InitializeAsync();

            return (manager, locations);
        }
    }
}
