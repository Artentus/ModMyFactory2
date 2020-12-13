//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal class ShortcutPropertiesViewModel : ViewModelBase<ShortcutPropertiesWindow>
    {
        private bool _useModpack, _useSavegame, _useCustomArgs;
        private Modpack? _selectedModpack;
        private string _savegamePath, _customArgs;

        public ICommand CreateCommand { get; }

        public ICommand CancelCommand { get; }

        public bool UseModpack
        {
            get => _useModpack;
            set
            {
                this.RaiseAndSetIfChanged(ref _useModpack, value, nameof(UseModpack));
                this.RaisePropertyChanged(nameof(CanCreate));
            }
        }

        public bool UseSavegame
        {
            get => _useSavegame;
            set
            {
                this.RaiseAndSetIfChanged(ref _useSavegame, value, nameof(UseSavegame));
                this.RaisePropertyChanged(nameof(CanCreate));
            }
        }

        public bool UseCustomArgs
        {
            get => _useCustomArgs;
            set
            {
                this.RaiseAndSetIfChanged(ref _useCustomArgs, value, nameof(UseCustomArgs));
                this.RaisePropertyChanged(nameof(CanCreate));
            }
        }

        public Modpack? SelectedModpack
        {
            get => _selectedModpack;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedModpack, value, nameof(SelectedModpack));
                this.RaisePropertyChanged(nameof(CanCreate));
            }
        }

        public string SavegamePath
        {
            get => _savegamePath;
            set
            {
                this.RaiseAndSetIfChanged(ref _savegamePath, value, nameof(SavegamePath));
                this.RaisePropertyChanged(nameof(CanCreate));
            }
        }

        public string CustomArgs
        {
            get => _customArgs;
            set
            {
                this.RaiseAndSetIfChanged(ref _customArgs, value, nameof(CustomArgs));
                this.RaisePropertyChanged(nameof(CanCreate));
            }
        }

        public bool CanCreate
        {
            get
            {
                if (UseModpack && (SelectedModpack is null)) return false;
                if (UseSavegame && string.IsNullOrWhiteSpace(SavegamePath)) return false;
                if (UseCustomArgs && string.IsNullOrWhiteSpace(CustomArgs)) return false;
                return true;
            }
        }

        public CollectionView<Modpack> Modpacks { get; }

        public ShortcutPropertiesViewModel()
        {
            Modpacks = new CollectionView<Modpack>(Program.Modpacks, new ModpackComparer());

            _savegamePath = string.Empty;
            _customArgs = string.Empty;

            CreateCommand = ReactiveCommand.Create(Create);
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        private void Create() => AttachedView!.Close(DialogResult.Ok);

        private void Cancel() => AttachedView!.Close(DialogResult.Cancel);
    }
}
