//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using System;
using System.Diagnostics;
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
        /// <param name="arguments">Optional command line arguments</param>
        Process Start(string arguments = null);
    }

    public static class FactorioInstance
    {
        /// <summary>
        /// Starts this instance
        /// </summary>
        /// <param name="modDirectory">
        /// Optional<br/>The mod directory to be used<br/>
        /// Overrides the instances default mod directory
        /// </param>
        /// <param name="savegameFile">
        /// Optional<br/>The savegame to load
        /// </param>
        /// <param name="arguments">
        /// Optional command line arguments
        /// </param>
        public static Process Start(this IFactorioInstance instance, in DirectoryInfo? modDirectory = null, in FileInfo? savegameFile = null, params string[] arguments)
        {
            var builder = new ArgumentBuilder();

            if (!(modDirectory is null))
            {
                builder.AppendArgument("--mod-directory");
                builder.AppendArgument(modDirectory.FullName);
            }

            if (!(savegameFile is null))
            {
                builder.AppendArgument("--load-game");
                builder.AppendArgument(savegameFile.FullName);
            }

            if (!(arguments is null))
            {
                builder.AppendArguments(arguments);
            }

            return instance.Start(builder.ToString());
        }

        /// <summary>
        /// Starts this instance
        /// </summary>
        /// <param name="modDirectory">
        /// Optional<br/>The mod directory to be used<br/>
        /// Overrides the instances default mod directory
        /// </param>
        /// <param name="savegameFile">
        /// Optional<br/>The savegame to load
        /// </param>
        /// <param name="arguments">
        /// Optional command line arguments
        /// </param>
        public static Process Start(this IFactorioInstance instance, in DirectoryInfo? modDirectory = null, in FileInfo? savegameFile = null, string? arguments = null)
        {
            var builder = new ArgumentBuilder();

            if (!(modDirectory is null))
            {
                builder.AppendArgument("--mod-directory");
                builder.AppendArgument(modDirectory.FullName);
            }

            if (!(savegameFile is null))
            {
                builder.AppendArgument("--load-game");
                builder.AppendArgument(savegameFile.FullName);
            }

            if (!string.IsNullOrEmpty(arguments))
            {
                builder.AppendExisting(arguments);
            }

            return instance.Start(builder.ToString());
        }

        /// <summary>
        /// Starts this instance
        /// </summary>
        /// <param name="arguments">Optional command line arguments</param>
        public static Process Start(this IFactorioInstance instance, params string[] arguments)
            => instance.Start(null, null, arguments);

        /// <summary>
        /// Checkes if this instance is the Steam instance
        /// </summary>
        public static bool IsSteamInstance(this IFactorioInstance instance)
        {
            var actualInstance = instance;
            if (instance is ManagedFactorioInstance managedInstance)
                actualInstance = managedInstance._baseInstance;
            return actualInstance is FactorioSteamInstance;
        }
    }
}
