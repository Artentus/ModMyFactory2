//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactoryGUI.Helpers;
using ReactiveUI;
using System;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModpackExportViewModel : ReactiveObject, IDisposable
    {
        public Modpack Modpack { get; }

        // Store information for fuzzy search
        public bool MatchesSearch { get; private set; } = true;

        public int SearchScore { get; private set; } = 0;

        public string DisplayName => Modpack.DisplayName;

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
                {
                }

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
