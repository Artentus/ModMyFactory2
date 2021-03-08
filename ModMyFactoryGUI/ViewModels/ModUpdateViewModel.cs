//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ReactiveUI;
using System.Collections.Generic;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModUpdateViewModel : ReactiveObject
    {
        public AccurateVersion FactorioVersion { get; }

        public string Header
        {
            get
            {
                if (FactorioVersion == (0, 18)) return "1.0 (0.18)";
                else return FactorioVersion.ToString(2);
            }
        }

        public CollectionView<ModUpdateInfo> Updates { get; }

        public ModUpdateViewModel(AccurateVersion factorioVersion, ICollection<ModUpdateInfo> updates)
        {
            FactorioVersion = factorioVersion;

            static int CompareNames(ModUpdateInfo first, ModUpdateInfo second)
                => first.Family.DisplayName.CompareTo(second.Family.DisplayName);
            Updates = new CollectionView<ModUpdateInfo>(updates, CompareNames);
        }
    }
}
