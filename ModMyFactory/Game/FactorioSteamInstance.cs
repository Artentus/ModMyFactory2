//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Mods;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ModMyFactory.Game
{
    internal sealed class FactorioSteamInstance : FactorioInstanceBase
    {
        private static readonly string SavegamePath;
        private static readonly string ScenarioPath;
        private static readonly string ModPath;

        static FactorioSteamInstance()
        {
            string appDataPath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".factorio");
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            SavegamePath = Path.Combine(appDataPath, "saves");
            ScenarioPath = Path.Combine(appDataPath, "scenarios");
            ModPath = Path.Combine(appDataPath, "mods");
        }


        private readonly Steam _steam;

        internal FactorioSteamInstance(DirectoryInfo directory, IModFile coreMod, IModFile baseMod, Steam steam)
            : base(directory, coreMod, baseMod,
                  new DirectoryInfo(SavegamePath),
                  new DirectoryInfo(ScenarioPath),
                  new DirectoryInfo(ModPath))
        {
            _steam = steam;
        }

        public override Process Start(string arguments) => _steam.StartApp(SteamApp.Factorio, arguments);
    }
}
