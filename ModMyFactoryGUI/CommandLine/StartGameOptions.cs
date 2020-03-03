//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using CommandLine;

namespace ModMyFactoryGUI.CommandLine
{
    [Verb("start-game", HelpText = "Starts a Factorio instance.")]
    sealed class StartGameOptions : OptionsBase
    {
        [Option("uid", Required = true, SetName = "_uid", HelpText = "The unique ID of the Factorio instance to start.")]
        public string Uid { get; }

        [Option("name", Required = true, SetName = "_name", HelpText = "Name of the Factorio instance to start.")]
        public string Name { get; }

        public StartGameOptions(string uid, string name, bool verbose, bool noLog, string appDataPath, bool noAutoUpdate)
            : base(verbose, noLog, appDataPath, noAutoUpdate)
            => (Uid, Name) = (uid, name);
    }
}
