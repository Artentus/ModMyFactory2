//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ManagerViewModel : MainViewModelBase<ManagerView>
    {
        private sealed class ModpackComparer : IComparer<ModpackViewModel>
        {
            public int Compare(ModpackViewModel first, ModpackViewModel second)
            {
                // Search score always takes precendence over the default alphabeticaal sorting
                int result = second.SearchScore.CompareTo(first.SearchScore);
                if (result == 0) result = first.DisplayName.CompareTo(second.DisplayName);
                return result;
            }
        }


        private readonly ObservableCollection<ModVersionGroupingViewModel> _modVersionGroupings;
        private readonly ObservableCollection<ModpackViewModel> _modpacks;
        private string _modFilter, _modpackFilter;

        public CollectionView<ModVersionGroupingViewModel> ModVersionGroupings { get; }

        public CollectionView<ModpackViewModel> Modpacks { get; }

        public string ModFilter
        {
            get => _modFilter;
            set
            {
                if (value != _modFilter)
                {
                    _modFilter = value;
                    this.RaisePropertyChanged(nameof(ModFilter));

                    foreach (var vm in _modVersionGroupings)
                        vm.Filter = value;
                }
            }
        }

        public string ModpackFilter
        {
            get => _modpackFilter;
            set
            {
                if (value != _modpackFilter)
                {
                    _modpackFilter = value;
                    this.RaisePropertyChanged(nameof(ModpackFilter));

                    foreach (var vm in _modpacks)
                        vm.ApplyFuzzyFilter(_modpackFilter);

                    Modpacks.Refresh();
                    this.RaisePropertyChanged(nameof(Modpacks));
                }
            }
        }

        public ManagerViewModel()
        {
            _modVersionGroupings = new ObservableCollection<ModVersionGroupingViewModel>();
            _modpacks = new ObservableCollection<ModpackViewModel>();

            foreach (var modManager in Program.Manager.ModManagers)
            {
                var vm = new ModVersionGroupingViewModel(modManager);
                _modVersionGroupings.Add(vm);
            }

            foreach (var modpack in Program.Modpacks)
            {
                var vm = new ModpackViewModel(modpack);
                _modpacks.Add(vm);
            }


            static int CompareVersionGroupings(ModVersionGroupingViewModel first, ModVersionGroupingViewModel second)
                => first.FactorioVersion.CompareTo(second.FactorioVersion);
            ModVersionGroupings = new CollectionView<ModVersionGroupingViewModel>(_modVersionGroupings, CompareVersionGroupings);

            Modpacks = new CollectionView<ModpackViewModel>(_modpacks, new ModpackComparer(), FilterModpack);


            Program.Manager.ModManagerCreated += OnModManagerCreated;
            Program.Modpacks.CollectionChanged += OnModpackCollectionChanged;
        }

        private bool FilterModpack(ModpackViewModel modpack)
        {
            // Filter based on fuzzy search
            return modpack.MatchesSearch;
        }

        private void OnModManagerCreated(object sender, ModManagerCreatedEventArgs e)
        {
            var vm = new ModVersionGroupingViewModel(e.ModManager);
            _modVersionGroupings.Add(vm);
        }

        private bool TryGetViewModel(Modpack modpack, out ModpackViewModel result)
        {
            foreach (var vm in _modpacks)
            {
                if (vm.Modpack == modpack)
                {
                    result = vm;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private void OnModpackCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Modpack modpack in e.NewItems)
                    {
                        var vm = new ModpackViewModel(modpack);
                        _modpacks.Add(vm);
                    }
                    this.RaisePropertyChanged(nameof(Modpacks));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Modpack modpack in e.OldItems)
                    {
                        if (TryGetViewModel(modpack, out var vm))
                            _modpacks.Remove(vm);
                    }
                    this.RaisePropertyChanged(nameof(Modpacks));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _modpacks.Clear();
                    this.RaisePropertyChanged(nameof(Modpacks));
                    break;
            }
        }

        protected override List<IMenuItemViewModel> GetEditMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }

        protected override List<IMenuItemViewModel> GetFileMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }
    }
}
