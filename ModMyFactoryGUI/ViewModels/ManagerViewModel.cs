//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using ModMyFactory.WebApi;
using ModMyFactory.WebApi.Mods;
using ModMyFactoryGUI.Caching.Web;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Tasks.Web;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ManagerViewModel : MainViewModelBase<ManagerView>
    {
        private readonly DownloadQueue _downloadQueue;
        private readonly ObservableCollection<ModVersionGroupingViewModel> _modVersionGroupings;
        private readonly ObservableCollection<ModpackViewModel> _modpacks;
        private string _modFilter, _modpackFilter;
        private bool? _allModsEnabled, _allModpacksEnabled;
        private bool _isUpdating;
        private volatile bool _isAdding, _isRemoving;

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

        public bool? AllModsEnabled
        {
            get => _allModsEnabled;
            set
            {
                if (value.HasValue)
                {
                    this.RaiseAndSetIfChanged(ref _allModsEnabled, value, nameof(AllModsEnabled));

                    _isUpdating = true;
                    foreach (var grouping in _modVersionGroupings)
                    {
                        foreach (var family in grouping.FamilyViewModels)
                            family.IsEnabled = value.Value;
                    }
                    _isUpdating = false;
                }
            }
        }

        public bool? AllModpacksEnabled
        {
            get => _allModpacksEnabled;
            set
            {
                if (value.HasValue)
                {
                    this.RaiseAndSetIfChanged(ref _allModpacksEnabled, value, nameof(AllModpacksEnabled));

                    _isUpdating = true;
                    foreach (var pack in _modpacks)
                        pack.Enabled = value.Value;
                    _isUpdating = false;
                }
            }
        }

        public ICommand AddModsCommand { get; }

        public ICommand UpdateModsCommand { get; }

        public ICommand CreateModpackCommand { get; }

        public ICommand DeleteModpackCommand { get; }

        public ManagerViewModel(DownloadQueue downloadQueue)
        {
            _downloadQueue = downloadQueue;
            _modVersionGroupings = new ObservableCollection<ModVersionGroupingViewModel>();
            _modpacks = new ObservableCollection<ModpackViewModel>();

            foreach (var modManager in Program.Manager.ModManagers)
            {
                var vm = new ModVersionGroupingViewModel(modManager);
                vm.FamilyViewModels.CollectionChanged += OnVersionGroupingCollectionChanged;
                foreach (var family in vm.FamilyViewModels)
                    family.PropertyChanged += OnFamilyPropertyChanged;
                _modVersionGroupings.Add(vm);
            }

            foreach (var modpack in Program.Modpacks)
            {
                var vm = new ModpackViewModel(modpack);
                vm.PropertyChanged += OnModpackPropertyChanged;
                _modpacks.Add(vm);
            }


            static int CompareVersionGroupings(ModVersionGroupingViewModel first, ModVersionGroupingViewModel second)
                => second.FactorioVersion.CompareTo(first.FactorioVersion);
            ModVersionGroupings = new CollectionView<ModVersionGroupingViewModel>(_modVersionGroupings, CompareVersionGroupings);

            Modpacks = new CollectionView<ModpackViewModel>(_modpacks, new ModpackComparer(), FilterModpack);


            EvaluateModEnabledStates();
            EvaluateModpackEnabledStates();


            Program.Manager.ModManagerCreated += OnModManagerCreated;
            Program.Modpacks.CollectionChanged += OnModpackCollectionChanged;


            AddModsCommand = ReactiveCommand.CreateFromTask(AddModsAsync);
            UpdateModsCommand = ReactiveCommand.CreateFromTask(UpdateModsAsync);
            CreateModpackCommand = ReactiveCommand.Create(CreateModpack);
            DeleteModpackCommand = ReactiveCommand.CreateFromTask<Modpack>(DeleteModpack);
        }

        private bool FilterModpack(ModpackViewModel modpack)
        {
            // Filter based on fuzzy search
            return modpack.MatchesSearch;
        }

        private void OnModManagerCreated(object sender, ModManagerCreatedEventArgs e)
        {
            var vm = new ModVersionGroupingViewModel(e.ModManager);
            vm.FamilyViewModels.CollectionChanged += OnVersionGroupingCollectionChanged;
            foreach (var family in vm.FamilyViewModels)
                family.PropertyChanged += OnFamilyPropertyChanged;
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

        private void EvaluateModEnabledStates()
        {
            _allModsEnabled = _modVersionGroupings
                .SelectMany(grouping => grouping.FamilyViewModels)
                .SelectFromAll(family => family.IsEnabled);
            this.RaisePropertyChanged(nameof(AllModsEnabled));
        }

        private void EvaluateModpackEnabledStates()
        {
            _allModpacksEnabled = _modpacks.SelectFromAll(vm => vm.Enabled);
            this.RaisePropertyChanged(nameof(AllModpacksEnabled));
        }

        private void OnFamilyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModFamilyViewModel.IsEnabled))
            {
                if (!_isUpdating) EvaluateModEnabledStates();
            }
        }

        private void OnVersionGroupingCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ModFamilyViewModel vm in e.NewItems)
                        vm.PropertyChanged += OnFamilyPropertyChanged;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ModFamilyViewModel vm in e.OldItems)
                        vm.PropertyChanged -= OnFamilyPropertyChanged;
                    break;
            }

            EvaluateModEnabledStates();
        }

        private void OnModpackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModpackViewModel.IsRenaming))
            {
                var vm = (ModpackViewModel)sender;
                if (!vm.IsRenaming) Modpacks.Refresh();
            }
            else if (e.PropertyName == nameof(ModpackViewModel.Enabled))
            {
                EvaluateModpackEnabledStates();
            }
        }

        private void OnModpackAdded(Modpack modpack, out ModpackViewModel vm)
        {
            vm = new ModpackViewModel(modpack);
            vm.PropertyChanged += OnModpackPropertyChanged;
            _modpacks.Add(vm);

            EvaluateModpackEnabledStates();
        }

        private void OnModpackRemoved(Modpack modpack)
        {
            if (TryGetViewModel(modpack, out var vm))
            {
                vm.PropertyChanged -= OnModpackPropertyChanged;
                _modpacks.Remove(vm);
                vm.Dispose();

                EvaluateModpackEnabledStates();
            }
        }

        private void OnModpackCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (!_isAdding)
                    {
                        foreach (Modpack modpack in e.NewItems)
                            OnModpackAdded(modpack, out _);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (!_isRemoving)
                    {
                        foreach (Modpack modpack in e.OldItems)
                            OnModpackRemoved(modpack);
                    }
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
                // Silently ignore this
            }
            else
            {
                var modDir = Program.Locations.GetModDir(modFile.Info.FactorioVersion);
                var movedFile = await modFile.CopyToAsync(modDir.FullName);
                modFile.Dispose();

                var mod = new Mod(movedFile);
                Program.Manager.AddMod(mod);
            }
        }

        private async Task<IReadOnlyDictionary<AccurateVersion, List<ModUpdateInfo>>> SearchModUpdatesAsync(
            IProgress<double> progress, CancellationToken cancellationToken)
        {
            var result = new Dictionary<AccurateVersion, List<ModUpdateInfo>>();

            int familyCount = Program.Manager.ModManagers.Select(m => m.Families.Count).Sum();
            int familyIndex = 0;

            using var infoCache = new ModInfoCache();
            foreach (var modManager in Program.Manager.ModManagers)
            {
                var list = new List<ModUpdateInfo>();
                result.Add(modManager.FactorioVersion, list);

                foreach (var family in modManager.Families)
                {
                    if (cancellationToken.IsCancellationRequested) return null;
                    progress.Report((double)familyIndex / (double)familyCount);
                    familyIndex++;

                    var info = await infoCache.QueryAsync(family.FamilyName);
                    var latest = info?.GetLatestRelease(modManager.FactorioVersion);
                    if (!latest.HasValue) continue; // Silently skip in case the local version of a mod does not exist on the portal

                    // The default mod is also always the one with the highest version
                    if (latest.Value.Version > family.GetDefaultMod().Version)
                    {
                        // Update available
                        list.Add(new ModUpdateInfo(family, latest.Value));
                        Log.Debug("Update to version {0} found for mod family '{1}', Factorio version {2}", latest.Value.Version, family.FamilyName, modManager.FactorioVersion);
                    }
                }
            }

            progress.Report(1.0);

            return result;
        }

        private async Task UpdateModsAsync()
        {
            try
            {
                string title = (string)App.Current.Locales.GetResource("ModUpdateSearch_Title");
                string message = (string)App.Current.Locales.GetResource("ModUpdateSearch_Message");

                var progress = new Progress<double>();
                var cancellationSource = new CancellationTokenSource();
                var searchTask = SearchModUpdatesAsync(progress, cancellationSource.Token);
                await ProgressDialog.Show(title, message, searchTask, progress, 0.0, 1.0, cancellationSource, App.Current.MainWindow);

                var updates = searchTask.Result;
                int updateCount = updates.Values.Select(l => l.Count()).Sum();
                if (updateCount > 0)
                {
                    // Some updates are available

                    var vm = new ModUpdateWindowViewModel(updates);
                    var window = View.CreateAndAttach(vm);
                    await window.ShowDialog(App.Current.MainWindow);

                    if (vm.DialogResult == DialogResult.Ok)
                    {
                        var (success, username, token) = await App.Current.Credentials.TryLogInAsync();
                        if (success.IsTrue())
                        {
                            foreach (var update in updates.Values.Flatten())
                            {
                                if (update.Selected)
                                {
                                    var job = new UpdateModJob(update, vm.ReplaceUpdates, vm.RemoveOldMods, username, token);
                                    _downloadQueue.AddJob(job);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // No updates available
                    await Messages.NoModUpdates.Show();
                }
            }
            catch (ApiException ex)
            {
                await MessageHelper.ShowMessageForApiException(ex);
            }
        }

        private void CreateModpack()
        {
            _isAdding = true;
            ModpackFilter = string.Empty; // Clear filter to avoid the new modpack getting hidden

            var modpack = Program.CreateModpack();
            OnModpackAdded(modpack, out var vm);

            vm.IsRenaming = true;
            AttachedView.ScrollModpackIntoView(vm);

            _isAdding = false;
        }

        private async Task DeleteModpack(Modpack modpack)
        {
            var title = (string)App.Current.Locales.GetResource("DeleteConfirm_Title");
            var message = string.Format((string)App.Current.Locales.GetResource("DeleteConfirm_Modpack_Message"), modpack.DisplayName);
            var result = await MessageBox.Show(title, message, MessageKind.Question, DialogOptions.YesNo);
            if (result == DialogResult.Yes)
            {
                _isRemoving = true;

                Program.DeleteModpack(modpack);
                OnModpackRemoved(modpack);

                _isRemoving = false;
            }
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
