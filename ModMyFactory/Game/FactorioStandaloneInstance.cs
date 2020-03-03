//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Mods;
using System.Diagnostics;
using System.IO;

namespace ModMyFactory.Game
{
    sealed class FactorioStandaloneInstance : FactorioInstanceBase
    {
        readonly FileInfo _executable;

        internal FactorioStandaloneInstance(DirectoryInfo directory, IModFile coreMod, IModFile baseMod, FileInfo executable)
            : base(directory, coreMod, baseMod,
                  new DirectoryInfo(Path.Combine(directory.FullName, "saves")),
                  new DirectoryInfo(Path.Combine(directory.FullName, "scenarios")),
                  new DirectoryInfo(Path.Combine(directory.FullName, "mods")))
        {
            _executable = executable;
        }

        public override void Start(params string[] args)
        {
            var startInfo = new ProcessStartInfo(_executable.FullName, string.Join(" ", args));
            Process.Start(startInfo);
        }
    }
}
