//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using CommandLine;

namespace ModMyFactoryGUI.CommandLine
{
    internal abstract class OptionsBase
    {
        [Option('v', "verbose", SetName = "_log_active", HelpText = "Enables verbose logging")]
        public bool Verbose { get; }

        [Option('l', "no-log", SetName = "_log_inactive", HelpText = "Disables logging to file (console logging still enabled)")]
        public bool NoLog { get; }

        [Option('a', "app-data", HelpText = "Overrides the application data path")]
        public string? AppDataPath { get; }

        protected OptionsBase(bool verbose, bool noLog, string? appDataPath)
            => (Verbose, NoLog, AppDataPath) = (verbose, noLog, appDataPath);
    }
}
