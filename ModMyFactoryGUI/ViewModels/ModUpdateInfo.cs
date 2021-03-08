//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using ModMyFactory.WebApi.Mods;
using ReactiveUI;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModUpdateInfo : ReactiveObject
    {
        private bool _selected;

        public bool Selected
        {
            get => _selected;
            set => this.RaiseAndSetIfChanged(ref _selected, value, nameof(Selected));
        }

        public ModFamily Family { get; }

        public ModReleaseInfo Release { get; }

        public AccurateVersion CurrentVersion { get; }

        public AccurateVersion UpdateVersion { get; }

        public ModUpdateInfo(ModFamily family, ModReleaseInfo release)
        {
            _selected = true;
            (Family, Release) = (family, release);

            CurrentVersion = family.GetDefaultMod()!.Version;
            UpdateVersion = release.Version;
        }
    }
}
