//  Copyright (C) 2020-2021 Mathis Rech
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

        public int Compare(OnlineModViewModel? first, OnlineModViewModel? second)
        {
            if (first is null)
            {
                if (second is null) return 0;
                else return int.MinValue;
            }
            else if (second is null)
            {
                return int.MaxValue;
            }
            else
            {
                // Search score always takes precendence over any other sorting
                int result = second.SearchScore.CompareTo(first.SearchScore);
                if (result == 0) result = SubCompare(first, second);
                return result;
            }
        }

        protected abstract int SubCompare(Mod first, Mod second);

        public int Compare(Mod? first, Mod? second)
        {
            if (first is null)
            {
                if (second is null) return 0;
                else return int.MinValue;
            }
            else if (second is null)
            {
                return int.MaxValue;
            }
            else
            {
                return SubCompare(first, second);
            }
        }

        protected abstract int SubCompare(ModExportViewModel first, ModExportViewModel second);

        public int Compare(ModExportViewModel? first, ModExportViewModel? second)
        {
            if (first is null)
            {
                if (second is null) return 0;
                else return int.MinValue;
            }
            else if (second is null)
            {
                return int.MaxValue;
            }
            else
            {
                return SubCompare(first, second);
            }
        }
    }

    internal sealed class AlphabeticalModComparer : ModComparer
    {
        public static readonly AlphabeticalModComparer Instance = new AlphabeticalModComparer();

        public override string DisplayNameKey => "Compare_Alphabetical";

        private AlphabeticalModComparer()
        { }

        protected override int SubCompare(OnlineModViewModel first, OnlineModViewModel second)
            => first.DisplayName.CompareTo(second.DisplayName);

        protected override int SubCompare(Mod first, Mod second)
            => first.DisplayName.CompareTo(second.DisplayName);

        protected override int SubCompare(ModExportViewModel first, ModExportViewModel second)
            => first.DisplayName.CompareTo(second.DisplayName);
    }

    internal sealed class DownloadCountModComparer : ModComparer
    {
        public static readonly DownloadCountModComparer Instance = new DownloadCountModComparer();

        public override string DisplayNameKey => "Compare_DownloadCount";

        private DownloadCountModComparer()
        { }

        protected override int SubCompare(OnlineModViewModel first, OnlineModViewModel second)
            => second.DownloadCount.CompareTo(first.DownloadCount);

        protected override int SubCompare(Mod first, Mod second)
            => throw new NotSupportedException();

        protected override int SubCompare(ModExportViewModel first, ModExportViewModel second)
            => throw new NotSupportedException();
    }
}
