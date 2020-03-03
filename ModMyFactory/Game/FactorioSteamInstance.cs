//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Mods;
using System;
using System.IO;

namespace ModMyFactory.Game
{
    internal sealed class FactorioSteamInstance : FactorioInstanceBase
    {
        private static readonly string SavegamePath;
        private static readonly string ScenarioPath;
        private static readonly string ModPath;

        static FactorioSteamInstance()
        {
#if NETFULL
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");
#elif NETCORE
            string appDataPath;
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT)
                appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");
            else if (os.Platform == PlatformID.Unix)
                appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".factorio");
            else
                throw new PlatformException();
#endif
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

        public override void Start(params string[] args) => _steam.StartApp(SteamApp.Factorio, args);
    }
}
