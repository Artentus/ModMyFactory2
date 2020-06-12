//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactory.Mods;
using ModMyFactoryGUI.Helpers;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModpackExportViewModel : ReactiveObject, IDisposable
    {
        private readonly ObservableCollection<ModExportViewModel> _mods;
        private bool _isSelected, _isUpdating;
        private bool _useLatestVersion, _useFactorioVersion, _useSpecificVersion;
        private bool? _includeFile, _downloadNewer;

        public Modpack Modpack { get; }

        public CollectionView<ModExportViewModel> Mods { get; }

        // Store information for fuzzy search
        public bool MatchesSearch { get; private set; } = true;

        public int SearchScore { get; private set; } = 0;

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value, nameof(IsSelected));
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
                    foreach (var mod in _mods)
                        mod.UseLatestVersion = true;
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
                    foreach (var mod in _mods)
                        mod.UseFactorioVersion = true;
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
                    foreach (var mod in _mods)
                        mod.UseSpecificVersion = true;
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
                foreach (var mod in _mods)
                    mod.IncludeFile = value.Value;
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
                foreach (var mod in _mods)
                    mod.DownloadNewer = value.Value;
                _isUpdating = false;
            }
        }

        public string DisplayName => Modpack.DisplayName;

        public ModpackExportViewModel(Modpack modpack)
        {
            _mods = new ObservableCollection<ModExportViewModel>();
            foreach (var mod in modpack.Mods)
            {
                var vm = new ModExportViewModel(mod);
                vm.PropertyChanged += ModPropertyChangedHandler;
                _mods.Add(vm);
            }
            Mods = new CollectionView<ModExportViewModel>(_mods, new AlphabeticalModComparer());

            EvaluateProperties();
            _includeFile = _mods.SelectFromAll(vm => vm.IncludeFile);
            _downloadNewer = _mods.SelectFromAll(vm => vm.DownloadNewer);

            Modpack = modpack;
            modpack.CollectionChanged += ModpackCollectionChangedHandler;
            modpack.PropertyChanged += ModpackPropertyChangedHandler;
        }

        private void EvaluateProperties()
        {
            _useLatestVersion = true;
            _useFactorioVersion = true;
            _useSpecificVersion = true;

            foreach (var vm in _mods)
            {
                if (!vm.UseLatestVersion) _useLatestVersion = false;
                if (!vm.UseFactorioVersion) _useFactorioVersion = false;
                if (!vm.UseSpecificVersion) _useSpecificVersion = false;
            }

            this.RaisePropertyChanged(nameof(UseLatestVersion));
            this.RaisePropertyChanged(nameof(UseFactorioVersion));
            this.RaisePropertyChanged(nameof(UseSpecificVersion));
        }

        private void ModPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ModExportViewModel.UseLatestVersion):
                case nameof(ModExportViewModel.UseFactorioVersion):
                case nameof(ModExportViewModel.UseSpecificVersion):
                    if (!_isUpdating) EvaluateProperties();
                    break;

                case nameof(ModExportViewModel.IncludeFile):
                    if (!_isUpdating)
                    {
                        _includeFile = _mods.SelectFromAll(vm => vm.IncludeFile);
                        this.RaisePropertyChanged(nameof(IncludeFile));
                    }
                    break;

                case nameof(ModExportViewModel.DownloadNewer):
                    if (!_isUpdating)
                    {
                        _downloadNewer = _mods.SelectFromAll(vm => vm.DownloadNewer);
                        this.RaisePropertyChanged(nameof(DownloadNewer));
                    }
                    break;
            }

        }

        private bool TryGetViewModel(Mod mod, out ModExportViewModel result)
        {
            foreach (var vm in _mods)
            {
                if (vm.Mod == mod)
                {
                    result = vm;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private void ModpackCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ICanEnable item in e.NewItems)
                    {
                        if (item is Mod mod)
                        {
                            var vm = new ModExportViewModel(mod);
                            vm.PropertyChanged += ModPropertyChangedHandler;
                            _mods.Add(vm);
                        }
                    }
                    this.RaisePropertyChanged(nameof(Mods));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ICanEnable item in e.OldItems)
                    {
                        if (item is Mod mod)
                        {
                            if (TryGetViewModel(mod, out var vm))
                            {
                                _mods.Remove(vm);
                                vm.PropertyChanged -= ModPropertyChangedHandler;
                            }
                        }
                    }
                    this.RaisePropertyChanged(nameof(Mods));
                    break;
            }

            EvaluateProperties();
        }

        private void ModpackPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModMyFactory.Modpack.DisplayName))
                this.RaisePropertyChanged(nameof(DisplayName));
        }

        public void ApplyFuzzyFilter(in string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                MatchesSearch = true;
                SearchScore = 0;
                return;
            }

            // We allow searching for title only
            MatchesSearch = DisplayName.FuzzyMatch(filter, out int titleScore);
            if (MatchesSearch) SearchScore = titleScore;
        }

        #region IDisposable Support

        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Modpack.CollectionChanged -= ModpackCollectionChangedHandler;
                    Modpack.PropertyChanged -= ModpackPropertyChangedHandler;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}
