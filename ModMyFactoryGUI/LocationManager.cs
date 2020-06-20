//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Game;
using ModMyFactoryGUI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    internal enum Location
    {
        AppData,
        BinDir,
        Custom
    }

    internal sealed class LocationManager : NotifyPropertyChangedBase
    {
        private const string AppDataValue = "#appdata";
        private const string BinDirValue = "#bindir";

        private readonly Manager _manager;
        private readonly SettingManager _settingManager;
        private readonly DirectoryInfo _binDir, _dataDir;
        private readonly Random _rnd;

        private Location _factorioLocation, _modLocation;
        private string _customFactorioPath, _customModPath;

        public event EventHandler ModsReloaded;

        public Location FactorioLocation
        {
            get => _factorioLocation;
            private set
            {
                if (value != _factorioLocation)
                {
                    _factorioLocation = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FactorioLocation)));
                }
            }
        }

        public Location ModLocation
        {
            get => _modLocation;
            private set
            {
                if (value != _modLocation)
                {
                    _modLocation = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModLocation)));
                }
            }
        }

        public string CustomFactorioPath
        {
            get => _customFactorioPath;
            private set
            {
                if (!string.Equals(value, _customFactorioPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    _customFactorioPath = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(CustomFactorioPath)));
                }
            }
        }

        public string CustomModPath
        {
            get => _customModPath;
            private set
            {
                if (!string.Equals(value, _customModPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    _customModPath = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(CustomModPath)));
                }
            }
        }

        public LocationManager(Manager manager, SettingManager settingManager, DirectoryInfo binDir, DirectoryInfo dataDir)
        {
            _manager = manager;
            _settingManager = settingManager;
            (_binDir, _dataDir) = (binDir, dataDir);
            _rnd = new Random();

            string factorioLocationString = settingManager.Get(SettingName.FactorioLocation, AppDataValue);
            _factorioLocation = factorioLocationString switch
            {
                AppDataValue => Location.AppData,
                BinDirValue => Location.BinDir,
                _ => Location.Custom
            };
            if (_factorioLocation == Location.Custom) _customFactorioPath = factorioLocationString;

            string modLocationString = settingManager.Get(SettingName.ModLocation, AppDataValue);
            _modLocation = modLocationString switch
            {
                AppDataValue => Location.AppData,
                BinDirValue => Location.BinDir,
                _ => Location.Custom
            };
            if (_modLocation == Location.Custom) _customModPath = modLocationString;
        }

        private void OnModsReloaded(EventArgs e)
            => ModsReloaded?.Invoke(this, e);

        private DirectoryInfo GetLocationDir(Location location, string dirName, string customPath)
        {
            return location switch
            {
                Location.AppData => _dataDir.CreateSubdirectory(dirName),
                Location.BinDir => _binDir.CreateSubdirectory(dirName),
                Location.Custom => Directory.CreateDirectory(customPath),
                _ => throw new InvalidOperationException() // Shouldn't happen
            };
        }

        private DirectoryInfo GetFactorioDir(Location location, string customPath)
            => GetLocationDir(location, "Factorio", customPath);

        private DirectoryInfo GetModDir(Location location, string customPath)
            => GetLocationDir(location, "mods", customPath);

        private string GetLocationString(Location location, string customPath)
        {
            return location switch
            {
                Location.AppData => AppDataValue,
                Location.BinDir => BinDirValue,
                Location.Custom => customPath,
                _ => throw new InvalidOperationException() // Shouldn't happen
            };
        }

        private async Task<bool> TryMoveFactorioLocationInternalAsync(DirectoryInfo source, DirectoryInfo dest)
        {
            // We overwrite to avoid any nasty errors that could corrupt the entire manager state.
            // However since we do this we need to warn the user if they try to move to a location that already exists.
            if (await FileHelper.AssureDirectorySafeForMoveAsync(dest))
            {
                await FileHelper.MoveDirectoryWithStatusAsync(source, dest.FullName);
                return true;
            }

            return false;
        }

        private async Task<bool> TryMoveModLocationInternalAsync(DirectoryInfo source, DirectoryInfo dest)
        {
            // We overwrite to avoid any nasty errors that could corrupt the entire manager state.
            // However since we do this we need to warn the user if they try to move to a location that already exists.
            if (await FileHelper.AssureDirectorySafeForMoveAsync(dest))
            {
                await FileHelper.MoveDirectoryWithStatusAsync(source, dest.FullName);
                return true;
            }

            return false;
        }

        private Task SaveFactorioLocationAsync()
        {
            string locationString = GetLocationString(_factorioLocation, _customFactorioPath);
            _settingManager.Set(SettingName.FactorioLocation, locationString);
            return _settingManager.SaveAsync();
        }

        private Task SaveModLocationAsync()
        {
            string locationString = GetLocationString(_modLocation, _customModPath);
            _settingManager.Set(SettingName.ModLocation, locationString);
            return _settingManager.SaveAsync();
        }

        private string GenerateRandomName()
        {
            // Since we check for duplicates the names don't need to be very long,
            // which is good because we don't want to run into the path length limit
            const int length = 6;
            const string allowedChars = "abcdefghijklmnopqrstuvwxyz0123456789";

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int rndIndex = _rnd.Next(0, allowedChars.Length);
                char c = allowedChars[rndIndex];
                sb.Append(c);
            }

            return sb.ToString();
        }

        public DirectoryInfo GetFactorioDir()
            => GetFactorioDir(_factorioLocation, _customFactorioPath);

        public DirectoryInfo GetModDir()
            => GetModDir(_modLocation, _customModPath);

        public DirectoryInfo GetModDir(AccurateVersion factorioVersion)
            => GetModDir().CreateSubdirectory(factorioVersion.ToMajor().ToString(2));

        public async Task MoveFactorioLocationAsync(Location location, string customPath)
        {
            // The current Factorio directory
            var current = GetFactorioDir();

            // The desired new Factorio directory
            var newDir = GetFactorioDir(location, customPath);

            if (await TryMoveFactorioLocationInternalAsync(current, newDir))
            {
                // If successfull (either no action required or accepted by the user) we update the location
                FactorioLocation = location;
                CustomFactorioPath = customPath;

                // Clear
                var instances = new ManagedFactorioInstance[_manager.ManagedInstances.Count];
                _manager.ManagedInstances.CopyTo(instances, 0);
                _manager.ClearInstances();
                foreach (var instance in _manager.ManagedInstances)
                    instance.Dispose();

                var mods = _manager.ModManagers.SelectMany(manager => manager.Families).SelectMany(family => family);
                var modsCopy = mods.ToArray();
                _manager.ClearMods();
                foreach (var mod in modsCopy)
                    mod.Dispose();

                // Reload
                await _manager.LoadFactorioInstancesAsync(this, _settingManager);
                await _manager.LoadModsAsync(this); // We need to reload mods as well
                OnModsReloaded(EventArgs.Empty);

                await SaveFactorioLocationAsync();
            }
        }

        public async Task MoveModLocationAsync(Location location, string customPath)
        {
            // The current mod directory
            var current = GetModDir();

            // The desired new mod directory
            var newDir = GetModDir(location, customPath);

            if (await TryMoveModLocationInternalAsync(current, newDir))
            {
                // If successfull (either no action required or accepted by the user) we update the location
                ModLocation = location;
                CustomModPath = customPath;

                // Clear
                var mods = _manager.ModManagers.SelectMany(manager => manager.Families).SelectMany(family => family);
                var modsCopy = mods.ToArray();
                _manager.ClearMods();
                foreach (var mod in modsCopy)
                    mod.Dispose();

                // Reload
                await _manager.LoadModsAsync(this);
                OnModsReloaded(EventArgs.Empty);

                await SaveModLocationAsync();
            }
        }

        public string GenerateNewFactorioDirectoryName()
        {
            var names = new HashSet<string>();
            var dir = GetFactorioDir();
            foreach (var subDir in dir.EnumerateDirectories())
                names.Add(subDir.Name.ToLowerInvariant());

            string newName;
            do
            {
                newName = GenerateRandomName();
            } while (names.Contains(newName));

            return newName;
        }

        public async Task InitializeAsync()
        {
            await _manager.LoadFactorioInstancesAsync(this, _settingManager);
            await _manager.LoadModsAsync(this);
        }
    }
}
