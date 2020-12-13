//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using System.Collections.Generic;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModpackComparer : IComparer<ModpackViewModel>, IComparer<ModpackExportViewModel>, IComparer<Modpack>
    {
        public int Compare(ModpackViewModel? first, ModpackViewModel? second)
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
                // Search score always takes precendence over the default alphabetical sorting
                int result = second.SearchScore.CompareTo(first.SearchScore);
                if (result == 0) result = first.DisplayName.CompareTo(second.DisplayName);
                return result;
            }
        }

        public int Compare(ModpackExportViewModel? first, ModpackExportViewModel? second)
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
                // Search score always takes precendence over the default alphabetical sorting
                int result = second.SearchScore.CompareTo(first.SearchScore);
                if (result == 0) result = first.DisplayName.CompareTo(second.DisplayName);
                return result;
            }
        }

        public int Compare(Modpack? first, Modpack? second)
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
                return first.DisplayName.CompareTo(second.DisplayName);
            }
        }
    }
}
