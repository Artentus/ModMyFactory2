//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Input;
using ModMyFactory;
using ModMyFactory.Mods;
using ModMyFactoryGUI.Controls.Icons;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public ICommand AddModsCommand { get; }

        public ICommand CreateModpackCommand { get; }

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


            AddModsCommand = ReactiveCommand.CreateFromTask(AddModsAsync);
            CreateModpackCommand = ReactiveCommand.Create(CreateModpack);
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

        private void OnModpackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModpackViewModel.IsRenaming))
            {
                var vm = (ModpackViewModel)sender;
                if (!vm.IsRenaming) Modpacks.Refresh();
            }
        }

        private void OnModpackCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Modpack modpack in e.NewItems)
                    {
                        var vm = new ModpackViewModel(modpack);
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
                            vm.PropertyChanged -= OnModpackPropertyChanged;
                            _modpacks.Remove(vm);
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

        private async Task AddModsAsync()
        {
            var filter = new FileDialogFilter();
            filter.Extensions.Add("zip");
            filter.Name = (string)App.Current.Locales.GetResource("ArchiveFileType");

            var ofd = new OpenFileDialog { AllowMultiple = true };
            ofd.Filters.Add(filter);

            var paths = await ofd.ShowAsync(App.Current.MainWindow);
            if (!(paths is null) && (paths.Length > 0))
            {
                foreach (var path in paths)
                {
                    var file = new FileInfo(path);
                    if (file.Exists) await ImportModAsync(file.FullName);
                }
            }
        }

        private async Task ImportModFileAsync(IModFile modFile)
        {
            if (Program.Manager.ContainsMod(modFile.Info.Name, modFile.Info.Version))
            {
                // ToDo: show info message
            }
            else
            {
                var modDir = Program.Locations.GetModDir(modFile.Info.FactorioVersion);
                var movedFile = await modFile.CopyToAsync(modDir.FullName);

                var mod = new Mod(movedFile);
                Program.Manager.AddMod(mod);
            }
        }

        private void CreateModpack()
        {
            var modpack = Program.CreateModpack();
        }

        protected override List<IMenuItemViewModel> GetEditMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }

        protected override List<IMenuItemViewModel> GetFileMenuViewModels()
        {
            return new List<IMenuItemViewModel>
            {
                new MenuItemViewModel(AddModsCommand, new KeyGesture(Avalonia.Input.Key.O, KeyModifiers.Control), true, () => new AddModsIcon(), "AddModFilesMenuItem", "AddModFilesHotkey"),
                new MenuItemViewModel(CreateModpackCommand, new KeyGesture(Avalonia.Input.Key.N, KeyModifiers.Control), true, () => new NewModpackIcon(), "NewModpackMenuItem", "NewModpackHotkey")
            };
        }

        public async Task ImportModAsync(string path)
        {
            var (success, modFile) = await ModFile.TryLoadAsync(path);
            if (success)
            {
                await ImportModFileAsync(modFile);
            }
            else
            {
                // ToDo: show error message
            }
        }
    }
}
