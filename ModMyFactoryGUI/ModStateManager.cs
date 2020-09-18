//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    class ModStateManager
    {
        private readonly LocationManager _locations;
        private readonly Manager _manager;
        private readonly Dictionary<AccurateVersion, SemaphoreSlim> _syncs = new Dictionary<AccurateVersion, SemaphoreSlim>();

        private SemaphoreSlim GetSync(AccurateVersion factorioVersion)
        {
            factorioVersion = factorioVersion.ToFactorioMajor();
            if (!_syncs.TryGetValue(factorioVersion, out var sync))
            {
                sync = new SemaphoreSlim(1, 1);
                _syncs.Add(factorioVersion, sync);
            }
            return sync;
        }

        public ModStateManager(LocationManager locations, Manager manager)
            => (_locations, _manager) = (locations, manager);

        public void SaveModList(AccurateVersion factorioVersion)
        {
            var modDir = _locations.GetModDir(factorioVersion);
            if (!modDir.Exists) modDir.Create();

            var modManager = _manager.GetModManager(factorioVersion);
            var state = ModFamilyStateGrouping.FromManager(modManager);

            var sync = GetSync(factorioVersion);
            sync.Wait();
            try
            {
                state.SaveToFile(Path.Combine(modDir.FullName, "mod-list.json"));
            }
            finally
            {
                sync.Release();
            }
        }

        public async Task SaveModListAsync(AccurateVersion factorioVersion)
        {
            var modDir = _locations.GetModDir(factorioVersion);
            if (!modDir.Exists) modDir.Create();

            var modManager = _manager.GetModManager(factorioVersion);
            var state = ModFamilyStateGrouping.FromManager(modManager);

            var sync = GetSync(factorioVersion);
            await sync.WaitAsync();
            try
            {
                await state.SaveToFileAsync(Path.Combine(modDir.FullName, "mod-list.json"));
            }
            finally
            {
                sync.Release();
            }
        }
    }
}
