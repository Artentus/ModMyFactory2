//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using System;
using System.IO;

namespace ModMyFactory.Game
{
    /// <summary>
    /// A Factorio instance
    /// </summary>
    public interface IFactorioInstance : IDisposable
    {
        /// <summary>
        /// The directory the instance is stored in
        /// </summary>
        DirectoryInfo Directory { get; }

        /// <summary>
        /// The Factorio core mod
        /// </summary>
        IModFile CoreMod { get; }

        /// <summary>
        /// The Factorio base mod
        /// </summary>
        IModFile BaseMod { get; }

        /// <summary>
        /// The version of the instance
        /// </summary>
        AccurateVersion Version { get; }

        /// <summary>
        /// The directory this instance stores its save data in
        /// </summary>
        DirectoryInfo SavegameDirectory { get; }

        /// <summary>
        /// The directory this instance loads scenarios from
        /// </summary>
        DirectoryInfo ScenarioDirectory { get; }

        /// <summary>
        /// The directory this instance is loading mods from
        /// </summary>
        DirectoryInfo ModDirectory { get; }

        /// <summary>
        /// Starts this instance
        /// </summary>
        /// <param name="args">Optional command line arguments</param>
        void Start(params string[] args);
    }

    public static class FactorioInstance
    {
        /// <summary>
        /// Starts this instance and overrides its mod directory
        /// </summary>
        /// <param name="modDirectory">The mod directory to be used; overrides the instances default mod directory</param>
        /// <param name="args">Optional command line arguments</param>
        public static void Start(this IFactorioInstance instance, DirectoryInfo modDirectory, params string[] args)
        {
            int argCount = (args?.Length ?? 0) + 2;
            var actualArgs = new string[argCount];
            args?.CopyTo(actualArgs, 2);

            actualArgs[0] = "--mod-directory";
            actualArgs[1] = $"\"{modDirectory.FullName}\"";

            instance.Start(actualArgs);
        }
    }
}
