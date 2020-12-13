//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using ModMyFactory;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ExportViewModel : MainViewModelBase<ExportView>
    {
        private readonly ObservableCollection<ModpackExportViewModel> _modpacks;
        private string? _filter;
        private int _exportCount;
        private bool _isUpdating;
        private bool _useLatestVersion, _useFactorioVersion, _useSpecificVersion;
        private bool? _includeFile, _downloadNewer;

        public CollectionView<ModpackExportViewModel> Modpacks { get; }

        public ICommand ExportCommand { get; }

        public bool CanExport { get; private set; }

        public string? Filter
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

        public bool UseLatestVersion
        {
            get => _useLatestVersion;
            set
            {
                this.RaiseAndSetIfChanged(ref _useLatestVersion, value, nameof(UseLatestVersion));

                if (value)
                {
                    _isUpdating = true;
                    foreach (var vm in _modpacks)
                        vm.UseLatestVersion = true;
                    _isUpdating = false;
                }
            }
        }

        public bool UseFactorioVersion
        {
            get => _useFactorioVersion;
            set
            {
                this.RaiseAndSetIfChanged(ref _useFactorioVersion, value, nameof(UseFactorioVersion));

                if (value)
                {
                    _isUpdating = true;
                    foreach (var vm in _modpacks)
                        vm.UseFactorioVersion = true;
                    _isUpdating = false;
                }
            }
        }

        public bool UseSpecificVersion
        {
            get => _useSpecificVersion;
            set
            {
                this.RaiseAndSetIfChanged(ref _useSpecificVersion, value, nameof(UseSpecificVersion));

                if (value)
                {
                    _isUpdating = true;
                    foreach (var vm in _modpacks)
                        vm.UseSpecificVersion = true;
                    _isUpdating = false;
                }
            }
        }

        public bool? IncludeFile
        {
            get => _includeFile;
            set
            {
                if (!value.HasValue) throw new ArgumentNullException();

                this.RaiseAndSetIfChanged(ref _includeFile, value, nameof(IncludeFile));

                _isUpdating = true;
                foreach (var vm in _modpacks)
                    vm.IncludeFile = value.Value;
                _isUpdating = false;
            }
        }

        public bool? DownloadNewer
        {
            get => _downloadNewer;
            set
            {
                if (!value.HasValue) throw new ArgumentNullException();

                this.RaiseAndSetIfChanged(ref _downloadNewer, value, nameof(DownloadNewer));

                _isUpdating = true;
                foreach (var vm in _modpacks)
                    vm.DownloadNewer = value.Value;
                _isUpdating = false;
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

            ExportCommand = ReactiveCommand.CreateFromTask(ExportAsync);

            EvaluateProperties();
            _includeFile = _modpacks.SelectFromAll(vm => vm.IncludeFile);
            _downloadNewer = _modpacks.SelectFromAll(vm => vm.DownloadNewer);
        }

        private bool FilterModpack(ModpackExportViewModel modpack)
        {
            // Filter based on fuzzy search
            return modpack.MatchesSearch;
        }

        private void EvaluateProperties()
        {
            _useLatestVersion = true;
            _useFactorioVersion = true;
            _useSpecificVersion = true;

            foreach (var vm in _modpacks)
            {
                if (!vm.UseLatestVersion) _useLatestVersion = false;
                if (!vm.UseFactorioVersion) _useFactorioVersion = false;
                if (!vm.UseSpecificVersion) _useSpecificVersion = false;
            }

            this.RaisePropertyChanged(nameof(UseLatestVersion));
            this.RaisePropertyChanged(nameof(UseFactorioVersion));
            this.RaisePropertyChanged(nameof(UseSpecificVersion));
        }

        private void OnModpackPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ModpackExportViewModel.DisplayName):
                    Modpacks.Refresh();
                    break;

                case nameof(ModpackExportViewModel.IsSelected):
                    // Counting is more efficient than iterating the entire list every time
                    var vm = (ModpackExportViewModel)sender!;
                    if (vm.IsSelected) _exportCount++;
                    else _exportCount--;

                    CanExport = _exportCount > 0;
                    this.RaisePropertyChanged(nameof(CanExport));
                    break;

                case nameof(ModpackExportViewModel.UseLatestVersion):
                case nameof(ModpackExportViewModel.UseFactorioVersion):
                case nameof(ModpackExportViewModel.UseSpecificVersion):
                    if (!_isUpdating) EvaluateProperties();
                    break;

                case nameof(ModpackExportViewModel.IncludeFile):
                    if (!_isUpdating)
                    {
                        _includeFile = _modpacks.SelectFromAll(vm => vm.IncludeFile);
                        this.RaisePropertyChanged(nameof(IncludeFile));
                    }
                    break;

                case nameof(ModpackExportViewModel.DownloadNewer):
                    if (!_isUpdating)
                    {
                        _downloadNewer = _modpacks.SelectFromAll(vm => vm.DownloadNewer);
                        this.RaisePropertyChanged(nameof(DownloadNewer));
                    }
                    break;
            }
        }

        private bool TryGetViewModel(Modpack modpack, [NotNullWhen(true)] out ModpackExportViewModel? result)
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

        private void OnModpackCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (!(e.NewItems is null))
                    {
                        foreach (Modpack modpack in e.NewItems)
                        {
                            var vm = new ModpackExportViewModel(modpack);
                            vm.PropertyChanged += OnModpackPropertyChanged;
                            _modpacks.Add(vm);
                        }
                    }
                    this.RaisePropertyChanged(nameof(Modpacks));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (!(e.OldItems is null))
                    {
                        foreach (Modpack modpack in e.OldItems)
                        {
                            if (TryGetViewModel(modpack, out var vm))
                            {
                                _modpacks.Remove(vm);
                                vm.PropertyChanged -= OnModpackPropertyChanged;
                                vm.Dispose();
                            }
                        }
                    }
                    this.RaisePropertyChanged(nameof(Modpacks));
                    break;
            }
        }

        #region Export

        private readonly Dictionary<Modpack, ModpackExportViewModel> _vmMappings
            = new Dictionary<Modpack, ModpackExportViewModel>();

        private void AddSubModpacksToExport(ModpackExportViewModel vm, HashSet<ModpackExportViewModel> set, Dictionary<Modpack, ModpackExportViewModel> mappings)
        {
            foreach (var subPack in vm.Modpack.Modpacks)
            {
                var subVm = mappings[subPack];
                set.Add(subVm);
                AddSubModpacksToExport(subVm, set, mappings);
            }
        }

        private ICollection<ModpackExportViewModel> GetModpacksToExport()
        {
            // Build the required mappings beforehand
            var mappings = new Dictionary<Modpack, ModpackExportViewModel>();
            foreach (var vm in _modpacks)
                mappings.Add(vm.Modpack, vm);

            var result = new HashSet<ModpackExportViewModel>();
            foreach (var vm in _modpacks)
            {
                if (vm.IsSelected)
                {
                    result.Add(vm);
                    AddSubModpacksToExport(vm, result, mappings);
                }
            }
            return result;
        }

        private bool CheckIncludesFiles()
        {
            // Only when false are none of the mods including files
            return IncludeFile != false;
        }

        private async Task ExportAsync()
        {
            var sfd = new SaveFileDialog();

            var normalFilter = new FileDialogFilter();
            normalFilter.Extensions.Add("fmp");
            normalFilter.Name = (string)App.Current.Locales.GetResource("FmpFileType");
            var archiveFilter = new FileDialogFilter();
            archiveFilter.Extensions.Add("fmpa");
            archiveFilter.Name = (string)App.Current.Locales.GetResource("FmpaFileType");

            bool includesFiles = CheckIncludesFiles();
            if (includesFiles)
            {
                // Only archive export is supported
                sfd.Filters.Add(archiveFilter);
                sfd.DefaultExtension = archiveFilter.Extensions[0];
            }
            else
            {
                // Plain-text and archive export is supported
                sfd.Filters.Add(normalFilter);
                sfd.Filters.Add(archiveFilter);
                sfd.DefaultExtension = normalFilter.Extensions[0];
            }

            var path = await sfd.ShowAsync(App.Current.MainWindow);
            if (!string.IsNullOrEmpty(path))
            {
                // We need to check this because in Lunix the default extension is not automatically appended
                if (string.IsNullOrEmpty(Path.GetExtension(path))) path += "." + sfd.DefaultExtension;

                // ToDo: show progress dialog

                // Run on the thread pool since it's an expensive operation
                var helper = new ExportHelper(GetModpacksToExport());
                var factory = await Task.Run(helper.PrepareExportAsync);

                // Check if we are packing
                var pathExt = Path.GetExtension(path).Substring(1); // Remove the dot
                foreach (var ext in archiveFilter.Extensions)
                {
                    if (string.Equals(pathExt, ext, StringComparison.InvariantCultureIgnoreCase))
                        factory.Forcepack = true;
                }

                var exporter = factory.CreateExporter();
                await exporter.ExportAsync(path);

                // We need to clean up any temporary files we created
                foreach (var file in Program.TemporaryDirectory.EnumerateFiles())
                    file.Delete();
            }
        }

        #endregion Export
    }
}
