//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace ModMyFactoryGUI.CommandLine
{
    [Verb("start-game", HelpText = "Starts a Factorio instance")]
    internal sealed class StartGameOptions : OptionsBase
    {
        [Usage(ApplicationAlias = "ModMyFactory")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new Example[]
                {
                    new Example("Start a Factorio instance using a specific modpack and loading a specific save",
                        new StartGameOptions(null, "Instance-01", null, "Modpack-01", "Save-01.zip", null, false, false, null))
                };
            }
        }

        [Option("id", Required = true, SetName = "_id", HelpText = "The unique ID of the Factorio instance to start")]
        public string? Uid { get; }

        [Option("name", Required = true, SetName = "_name", HelpText = "Name of the Factorio instance to start (case sensitive)")]
        public string? Name { get; }

        [Option("pack-id", Default = null, HelpText = "Optional modpack ID")]
        public int? ModpackId { get; }

        [Option("pack-name", HelpText = "Optional modpack name (case sensitive)")]
        public string? ModpackName { get; }

        [Option("load-save", HelpText = "Optional save file to load")]
        public string? SavegameFile { get; }

        [Option("custom", HelpText = "Optional custom command line arguments that are passed to Factorio")]
        public string? CustomArguments { get; }

        public StartGameOptions(
            string? uid, string? name, int? modpackId, string? modpackName, string? savegameFile, string? customArguments,
            bool verbose, bool noLog, string? appDataPath)
            : base(verbose, noLog, appDataPath)
            => (Uid, Name, ModpackId, ModpackName, SavegameFile, CustomArguments)
                = (uid, name, modpackId, modpackName, savegameFile, customArguments);
    }
}
