//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Mods;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModFamilyViewModel : ReactiveObject
    {
        private readonly ModFamily _family;
        private readonly ObservableCollection<ModViewModel> _modViewModels;

        public ReadOnlyObservableCollection<ModViewModel> ModViewModels { get; }

        public string DisplayName => _family.DisplayName;

        public string FamilyName => _family.FamilyName;

        public ModFamilyViewModel(ModFamily family)
        {
            _family = family;
            _modViewModels = new ObservableCollection<ModViewModel>();
            ModViewModels = new ReadOnlyObservableCollection<ModViewModel>(_modViewModels);

            foreach (var mod in family)
            {
                var vm = new ModViewModel(mod);
                _modViewModels.Add(vm);
            }

            family.CollectionChanged += OnModCollectionChanged;
        }

        private void OnModCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Mod mod in e.NewItems)
                    {
                        var vm = new ModViewModel(mod);
                        _modViewModels.Add(vm);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Mod mod in e.OldItems)
                    {
                        // A bit inefficient but since there will only be a small amount of mods in a single family it shouldn't matter
                        var vm = _modViewModels.Where(item => item.Mod == mod).FirstOrDefault();
                        if (!(vm is null)) _modViewModels.Remove(vm);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _modViewModels.Clear();
                    break;
            }
        }
    }
}
