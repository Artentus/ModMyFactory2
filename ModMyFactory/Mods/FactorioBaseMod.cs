//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Game;

namespace ModMyFactory.Mods
{
    internal class FactorioBaseMod : Mod
    {
        public override bool CanDisable => false;

        public FactorioBaseMod(IFactorioInstance instance)
            : base(instance.BaseMod.Info.Name, instance.BaseMod.Info.DisplayName, instance.BaseMod.Info.Version, instance.BaseMod.Info.Version.ToMajor(),
                  instance.BaseMod.Info.Author, instance.BaseMod.Info.Description, instance.BaseMod.Info.Dependencies, instance.BaseMod.Thumbnail)
        { }
    }
}
