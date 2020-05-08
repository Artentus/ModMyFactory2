//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Media.Imaging;
using ModMyFactory.Mods;
using ModMyFactoryGUI.Helpers;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModFamilyViewModel : ReactiveObject, IDisposable
    {
        private readonly ModFamily _family;
        private readonly ObservableCollection<ModViewModel> _modViewModels;
        private bool _isEnabled;
        private ModViewModel _selectedModViewModel;

        public ReadOnlyObservableCollection<ModViewModel> ModViewModels { get; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;
                    if (value)
                    {
                        if (!(SelectedModViewModel is null))
                            SelectedModViewModel.Mod.Enabled = true;
                    }
                    else
                    {
                        foreach (var mod in _family)
                            mod.Enabled = false;
                    }
                    this.RaisePropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public ModViewModel SelectedModViewModel
        {
            get => _selectedModViewModel;
            set => this.RaiseAndSetIfChanged(ref _selectedModViewModel, value, nameof(SelectedModViewModel));
        }

        public IBitmap Thumbnail => _modViewModels.MaxBy(m => m.Version)?.Thumbnail;

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
            family.ModsEnabledChanged += OnModsEnabledChanged;

            RefreshEnabledState();
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
                        this.RaisePropertyChanged(nameof(Thumbnail));
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Mod mod in e.OldItems)
                    {
                        // A bit inefficient but since there will only be a small amount of mods in a single family it shouldn't matter
                        var vm = _modViewModels.Where(item => item.Mod == mod).FirstOrDefault();
                        if (!(vm is null))
                        {
                            _modViewModels.Remove(vm);
                            vm.Dispose();
                            this.RaisePropertyChanged(nameof(Thumbnail));
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _modViewModels.Clear();
                    this.RaisePropertyChanged(nameof(Thumbnail));
                    break;
            }
        }

        private void RefreshEnabledState()
        {
            if (_family.EnabledMod is null)
            {
                _isEnabled = false;
            }
            else
            {
                _isEnabled = true;
                SelectedModViewModel = _modViewModels.FirstOrDefault(vm => object.ReferenceEquals(vm.Mod, _family.EnabledMod));
            }
        }

        private void OnModsEnabledChanged(object sender, EventArgs e)
        {
            RefreshEnabledState();
            this.RaisePropertyChanged(nameof(IsEnabled));
        }

        #region IDisposable Support

        private bool disposed = false;

        ~ModFamilyViewModel()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var vm in _modViewModels)
                        vm.Dispose();

                    _family.CollectionChanged -= OnModCollectionChanged;
                    _family.ModsEnabledChanged += OnModsEnabledChanged;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
