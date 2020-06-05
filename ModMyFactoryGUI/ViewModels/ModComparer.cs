//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Mods;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ModMyFactoryGUI.ViewModels
{
    internal abstract class ModComparer : NotifyPropertyChangedBase, IComparer<Mod>, IComparer<OnlineModViewModel>, IComparer<ModExportViewModel>
    {
        public abstract string DisplayNameKey { get; }

        protected ModComparer()
        {
            // Technically we should unsubscribe from this however these object have
            // the same lifetime as the entire app so it doesn't really matter
            App.Current.Locales.UICultureChanged += (sender, e)
                => OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayNameKey)));
        }

        protected abstract int SubCompare(OnlineModViewModel first, OnlineModViewModel second);

        public int Compare(OnlineModViewModel first, OnlineModViewModel second)
        {
            // Search score always takes precendence over any other sorting
            int result = second.SearchScore.CompareTo(first.SearchScore);
            if (result == 0) result = SubCompare(first, second);
            return result;
        }

        public abstract int Compare(Mod first, Mod second);

        public abstract int Compare(ModExportViewModel first, ModExportViewModel second);
    }

    internal sealed class AlphabeticalModComparer : ModComparer
    {
        public override string DisplayNameKey => "Compare_Alphabetical";

        protected override int SubCompare(OnlineModViewModel first, OnlineModViewModel second)
            => first.DisplayName.CompareTo(second.DisplayName);

        public override int Compare(Mod first, Mod second)
                    => first.DisplayName.CompareTo(second.DisplayName);

        public override int Compare(ModExportViewModel first, ModExportViewModel second)
            => first.DisplayName.CompareTo(second.DisplayName);
    }

    internal sealed class DownloadCountModComparer : ModComparer
    {
        public override string DisplayNameKey => "Compare_DownloadCount";

        protected override int SubCompare(OnlineModViewModel first, OnlineModViewModel second)
            => second.DownloadCount.CompareTo(first.DownloadCount);

        public override int Compare(Mod first, Mod second)
                    => throw new NotSupportedException();

        public override int Compare(ModExportViewModel first, ModExportViewModel second)
            => throw new NotSupportedException();
    }
}
