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

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ManagerViewModel : MainViewModelBase<ManagerView>
    {
        private readonly ObservableCollection<ModVersionGroupingViewModel> _modVersionGroupings;
        private string _modFilter, _modpackFilter;

        public CollectionView<ModVersionGroupingViewModel> ModVersionGroupings { get; }

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
                }
            }
        }

        public ManagerViewModel()
        {
            _modVersionGroupings = new ObservableCollection<ModVersionGroupingViewModel>();

            static int CompareViewModels(ModVersionGroupingViewModel first, ModVersionGroupingViewModel second)
                => first.FactorioVersion.CompareTo(second.FactorioVersion);
            ModVersionGroupings = new CollectionView<ModVersionGroupingViewModel>(_modVersionGroupings, CompareViewModels);

            foreach (var modManager in Program.Manager.ModManagers)
            {
                var vm = new ModVersionGroupingViewModel(modManager);
                _modVersionGroupings.Add(vm);
            }

            Program.Manager.ModManagerCreated += OnModManagerCreated;
        }

        private void OnModManagerCreated(object sender, ModManagerCreatedEventArgs e)
        {
            var vm = new ModVersionGroupingViewModel(e.ModManager);
            _modVersionGroupings.Add(vm);
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
