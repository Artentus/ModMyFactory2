//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using System;
using System.Diagnostics;
using System.IO;

namespace ModMyFactory.Game
{
    internal abstract class FactorioInstanceBase : IFactorioInstance
    {
        public DirectoryInfo Directory { get; }

        public IModFile CoreMod { get; }

        public IModFile BaseMod { get; }

        public DirectoryInfo SavegameDirectory { get; }
        public DirectoryInfo ScenarioDirectory { get; }
        public DirectoryInfo ModDirectory { get; }
        public AccurateVersion Version => BaseMod.Info.Version;

        protected FactorioInstanceBase(DirectoryInfo directory, IModFile coreMod, IModFile baseMod,
            DirectoryInfo savegameDirectory, DirectoryInfo scenarioDirectory, DirectoryInfo modDirectory)
            => (Directory, CoreMod, BaseMod, SavegameDirectory, ScenarioDirectory, ModDirectory)
               = (directory, coreMod, baseMod, savegameDirectory, scenarioDirectory, modDirectory);

        ~FactorioInstanceBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CoreMod.Dispose();
                BaseMod.Dispose();
            }
        }

        public abstract Process Start(string arguments);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
