//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Game;
using System;
using System.Diagnostics;
using System.IO;

namespace ModMyFactory.Server
{
    public static class FactorioInstanceExtensions
    {
        /// <summary>
        /// Starts this instance as a headless server
        /// </summary>
        /// <param name="startOptions">
        /// Server options
        /// </param>
        /// <param name="modDirectory">
        /// Optional<br/>The mod directory to be used<br/>
        /// Overrides the instances default mod directory</param>
        /// <param name="arguments">
        /// Optional command line arguments
        /// </param>
        /// <returns></returns>
        public static Process StartServer(this IFactorioInstance instance, FileInfo savegameFile, ServerStartOptions startOptions, DirectoryInfo? modDirectory = null, params string[] arguments)
        {
            if (savegameFile is null) throw new ArgumentNullException(nameof(savegameFile));
            if (startOptions is null) throw new ArgumentNullException(nameof(startOptions));

            var fullArgs = startOptions.ToArgs(savegameFile);
            if (!(arguments is null) && (arguments.Length > 0)) fullArgs.AddRange(arguments);

            return instance.Start(modDirectory, null, fullArgs.ToArray());
        }
    }
}
