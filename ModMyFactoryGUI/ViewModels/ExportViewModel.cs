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
using System.ComponentModel;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ExportViewModel : MainViewModelBase<ExportView>
    {
        private readonly ObservableCollection<ModpackExportViewModel> _modpacks;
        private string _filter;

        public CollectionView<ModpackExportViewModel> Modpacks { get; }

        public string Filter
        {
            get => _filter;
            set
            {
                if (value != _filter)
                {
                    _filter = value;
                    this.RaisePropertyChanged(nameof(Filter));

                    foreach (var vm in _modpacks)
                        vm.ApplyFuzzyFilter(_filter);

                    Modpacks.Refresh();
                    this.RaisePropertyChanged(nameof(Modpacks));
                }
            }
        }

        public ExportViewModel()
        {
            _modpacks = new ObservableCollection<ModpackExportViewModel>();
            foreach (var modpack in Program.Modpacks)
            {
                var vm = new ModpackExportViewModel(modpack);
                vm.PropertyChanged += OnModpackPropertyChanged;
                _modpacks.Add(vm);
            }
            Modpacks = new CollectionView<ModpackExportViewModel>(_modpacks, new ModpackComparer(), FilterModpack);

            Program.Modpacks.CollectionChanged += OnModpackCollectionChanged;
        }

        private bool FilterModpack(ModpackExportViewModel modpack)
        {
            // Filter based on fuzzy search
            return modpack.MatchesSearch;
        }

        private void OnModpackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModpackExportViewModel.DisplayName))
                Modpacks.Refresh();
        }

        private bool TryGetViewModel(Modpack modpack, out ModpackExportViewModel result)
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
                        var vm = new ModpackExportViewModel(modpack);
                        vm.PropertyChanged += OnModpackPropertyChanged;
                        _modpacks.Add(vm);
                    }
                    this.RaisePropertyChanged(nameof(Modpacks));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Modpack modpack in e.OldItems)
                    {
                        if (TryGetViewModel(modpack, out var vm))
                        {
                            _modpacks.Remove(vm);
                            vm.PropertyChanged -= OnModpackPropertyChanged;
                            vm.Dispose();
                        }
                    }
                    this.RaisePropertyChanged(nameof(Modpacks));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (var vm in _modpacks)
                    {
                        vm.PropertyChanged -= OnModpackPropertyChanged;
                        vm.Dispose();
                    }
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
