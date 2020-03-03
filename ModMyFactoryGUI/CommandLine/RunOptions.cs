//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using CommandLine;

namespace ModMyFactoryGUI.CommandLine
{
    [Verb("run", HelpText = "Starts ModMyFactory GUI.")]
    internal sealed class RunOptions : OptionsBase
    {
        public RunOptions(bool verbose, bool noLog, string appDataPath, bool noAutoUpdate)
            : base(verbose, noLog, appDataPath, noAutoUpdate)
        {
        }
    }
}
