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
using System.Collections.Generic;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModpackViewModel : ReactiveObject, IDisposable
    {
        private bool _isRenaming;

        public Modpack Modpack { get; }

        public string DisplayName
        {
            get => Modpack.DisplayName;
            set
            {
                if (value != Modpack.DisplayName)
                {
                    Modpack.DisplayName = value;
                    this.RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        public bool? Enabled
        {
            get => Modpack.Enabled;
            set => Modpack.Enabled = value;
        }

        public bool IsRenaming
        {
            get => _isRenaming;
            set
            {
                if (this.RaiseAndSetIfChanged(ref _isRenaming, value, nameof(IsRenaming)) && !value)
                {
                    Program.SaveModpacks();
                }
            }
        }

        // Store information for fuzzy search
        public bool MatchesSearch { get; private set; } = true;

        public int SearchScore { get; private set; } = 0;

        public ICommand BeginRenameCommand { get; }
        public ICommand EndRenameCommand { get; }

        public ICommand RemoveModCommand { get; }

        public IEnumerable<Modpack> Modpacks => Modpack.Modpacks;

        public IEnumerable<Mod> Mods => Modpack.Mods;

        public ModpackViewModel(Modpack modpack)
        {
            Modpack = modpack;
            modpack.EnabledChanged += ModpackEnabledChangedHandler;

            BeginRenameCommand = ReactiveCommand.Create(() => IsRenaming = true);
            EndRenameCommand = ReactiveCommand.Create(() => IsRenaming = false);
            RemoveModCommand = ReactiveCommand.Create<ICanEnable>(RemoveMod);
        }

        private void ModpackEnabledChangedHandler(object sender, EventArgs e)
            => this.RaisePropertyChanged(nameof(Enabled));

        private void RemoveMod(ICanEnable mod)
        {
            if (!(mod is null))
            {
                Modpack.Remove(mod);
                this.RaisePropertyChanged(nameof(Mods));
                this.RaisePropertyChanged(nameof(Modpacks));
            }
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
                    Modpack.EnabledChanged -= ModpackEnabledChangedHandler;

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
