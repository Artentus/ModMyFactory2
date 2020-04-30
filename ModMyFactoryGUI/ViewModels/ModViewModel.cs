//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using ReactiveUI;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModViewModel : ReactiveObject
    {
        public Mod Mod { get; }

        public AccurateVersion Version => Mod.Version;

        public ModViewModel(Mod mod)
            => Mod = mod;
    }
}
