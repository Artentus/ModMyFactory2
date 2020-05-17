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

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModpackViewModel : ReactiveObject, IDisposable
    {
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

        // Store information for fuzzy search
        public bool MatchesSearch { get; private set; } = true;

        public int SearchScore { get; private set; } = 0;

        public IEnumerable<Modpack> Modpacks => Modpack.Modpacks;

        public IEnumerable<Mod> Mods => Modpack.Mods;

        public ModpackViewModel(Modpack modpack)
        {
            Modpack = modpack;
            modpack.EnabledChanged += ModpackEnabledChangedHandler;
        }

        private void ModpackEnabledChangedHandler(object sender, EventArgs e)
            => this.RaisePropertyChanged(nameof(Enabled));

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

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    Modpack.EnabledChanged -= ModpackEnabledChangedHandler;

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}
