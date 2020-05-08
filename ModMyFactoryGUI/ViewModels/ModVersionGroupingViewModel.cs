//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using ModMyFactoryGUI.Helpers;
using ReactiveUI;
using System;
using System.Collections.Specialized;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModVersionGroupingViewModel : ReactiveObject
    {
        private readonly ModManager _manager;
        private readonly ObservableDictionary<string, ModFamilyViewModel> _familyViewModels;

        public ObservableDictionary<string, ModFamilyViewModel>.ObservableValueCollection FamilyViewModels => _familyViewModels.Values;

        public AccurateVersion FactorioVersion => _manager.FactorioVersion;

        public ModVersionGroupingViewModel(ModManager manager)
        {
            _manager = manager;

            _familyViewModels = new ObservableDictionary<string, ModFamilyViewModel>();
            foreach (var family in manager.Families)
            {
                var vm = new ModFamilyViewModel(family);
                _familyViewModels.Add(family.FamilyName, vm);
            }

            manager.CollectionChanged += OnModCollectionChanged;
        }

        private void OnModCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Mod mod in e.NewItems)
                    {
                        // If the newly added mod is part of a new family we need to add a new view model
                        if (!_familyViewModels.ContainsKey(mod.Name))
                        {
                            if (_manager.TryGetFamily(mod.Name, out var family))
                            {
                                var vm = new ModFamilyViewModel(family);
                                _familyViewModels.Add(family.FamilyName, vm);
                            }
                            else
                            {
                                // Assuming the code works this will never be reached
                                throw new InvalidOperationException();
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Mod mod in e.OldItems)
                    {
                        // If the removed mod was the last mod in its family we need to remove the view model
                        if (_familyViewModels.ContainsKey(mod.Name))
                        {
                            if (!_manager.TryGetFamily(mod.Name, out var family) || family.Count == 0)
                            {
                                if (_familyViewModels.RemoveEx(mod.Name, out var vm))
                                    vm.Dispose();
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _familyViewModels.Clear();
                    break;
            }
        }
    }
}
