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
    internal sealed class FactorioStandaloneInstance : FactorioInstanceBase
    {
        private readonly FileInfo _executable;

        internal FactorioStandaloneInstance(DirectoryInfo directory, IModFile coreMod, IModFile baseMod, FileInfo executable)
            : base(directory, coreMod, baseMod,
                  new DirectoryInfo(Path.Combine(directory.FullName, "saves")),
                  new DirectoryInfo(Path.Combine(directory.FullName, "scenarios")),
                  new DirectoryInfo(Path.Combine(directory.FullName, "mods")))
        {
            _executable = executable;
        }

        public override Process Start(string arguments)
        {
            var startInfo = new ProcessStartInfo(_executable.FullName, arguments);
            return Process.Start(startInfo);
        }
    }
}
