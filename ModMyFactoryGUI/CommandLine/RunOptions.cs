//  Copyright (C) 2020-2021 Mathis Rech
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
    [Verb("run", true, HelpText = "Starts ModMyFactory GUI")]
    internal sealed class RunOptions : OptionsBase
    {
        [Usage(ApplicationAlias = "ModMyFactory")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new Example[]
                {
                    new Example("Import package files",
                        new RunOptions(false, false, new[] { "package_1.fmp", "package_2.fmpa" }, false, false, null))
                };
            }
        }

        [Option('u', "no-update", HelpText = "Disables the automatic update check")]
        public bool NoAutoUpdate { get; }

        [Option('f', "no-file-associations", HelpText = "Disables the creation of file type associations")]
        public bool NoFileTypeAssociations { get; }

        [Option('i', "import", Min = 1, Separator = ';', HelpText = "Optional list of package files to import")]
        public IEnumerable<string> ImportList { get; }

        public RunOptions(bool noAutoUpdate, bool noFileTypeAssociations, IEnumerable<string> importList, bool verbose, bool noLog, string? appDataPath)
            : base(verbose, noLog, appDataPath)
            => (NoAutoUpdate, NoFileTypeAssociations, ImportList) = (noAutoUpdate, noFileTypeAssociations, importList);
    }
}
