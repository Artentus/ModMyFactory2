//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactoryGUI.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ManagerViewModel : MainViewModelBase<ManagerView>
    {
        private readonly ObservableCollection<ModVersionGroupingViewModel> _modVersionGroupings;

        public CollectionView<ModVersionGroupingViewModel> ModVersionGroupings { get; }

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
