//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.IO;
using ModMyFactory.Mods;
using System;
using System.IO;

namespace ModMyFactory.Game
{
    /// <summary>
    /// A Factorio instance that is managed by ModMyFactory
    /// </summary>
    public sealed class ManagedFactorioInstance : IFactorioInstance
    {
        private readonly IFactorioInstance _baseInstance;

        /// <summary>
        /// Indicates whether this instance was created by an instance manager
        /// </summary>
        public bool HasManagerAttached { get; }

        /// <summary>
        /// The mod manager associated with this instance
        /// </summary>
        public ModManager ModManager { get; }

        public DirectoryInfo Directory => _baseInstance.Directory;

        public IModFile CoreMod => _baseInstance.CoreMod;

        public IModFile BaseMod => _baseInstance.BaseMod;

        public AccurateVersion Version => _baseInstance.Version;

        public DirectoryInfo SavegameDirectory => _baseInstance.SavegameDirectory;

        public DirectoryInfo ScenarioDirectory => _baseInstance.ScenarioDirectory;

        public DirectoryInfo ModDirectory => _baseInstance.ModDirectory;

        private ManagedFactorioInstance(IFactorioInstance baseInstance, ModManager modManager, bool hasManagerAttached)
        {
            _baseInstance = baseInstance;
            ModManager = modManager;
            HasManagerAttached = hasManagerAttached;
        }

        ~ManagedFactorioInstance()
        {
            Dispose(false);
        }

        private static void LinkDirectory(DirectoryInfo directory, string destination)
        {
            var link = Symlink.FromPath(directory.FullName);
            if (link.Exists)
            {
                link.DestinationPath = destination;
            }
            else
            {
                if (directory.Exists) directory.Delete(true);
                link.Create(destination);
            }
        }

        private static void UnlinkDirectory(DirectoryInfo directory)
        {
            var link = Symlink.FromPath(directory.FullName);
            if (link.Exists) link.Delete();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                _baseInstance.Dispose();
        }

        internal static ManagedFactorioInstance CreateInternal(IFactorioInstance instance, ModManager modManager)
        {
            if (instance is null) throw new ArgumentNullException(nameof(instance));
            if (modManager is null) throw new ArgumentNullException(nameof(modManager));

            if (instance is ManagedFactorioInstance)
                throw new InvalidOperationException("Already managed instances cannot be added");

            return new ManagedFactorioInstance(instance, modManager, true);
        }

        internal void LinkModDirectoryInternal(string destination) => LinkDirectory(ModDirectory, destination);

        internal void UnlinkModDirectoryInternal() => UnlinkDirectory(ModDirectory);

        /// <summary>
        /// Creates a managed Factorio instance from an existing instance
        /// The created instance will not be associated with an instance manager
        /// If the instance is already managed it will be returned directly
        /// </summary>
        public static ManagedFactorioInstance FromInstance(IFactorioInstance instance, ModManager modManager)
        {
            if (instance is null) throw new ArgumentNullException(nameof(instance));
            if (modManager is null) throw new ArgumentNullException(nameof(modManager));

            if (instance is ManagedFactorioInstance managedInstance)
            {
                if (managedInstance.ModManager != modManager)
                    throw new InvalidOperationException("Managed instance already has another associated mod manager");
                return managedInstance;
            }

            return new ManagedFactorioInstance(instance, modManager, false);
        }

        public void Start(string[] args) => _baseInstance.Start(args);

        /// <summary>
        /// Links this instances save directory to another location.
        /// All contents in the old directory will be deleted!
        /// </summary>
        /// <param name="destination">The location to link to.</param>
        public void LinkSavegameDirectory(string destination) => LinkDirectory(SavegameDirectory, destination);

        /// <summary>
        /// Unlinks this instances save directory so it points to its original location.
        /// </summary>
        public void UnlinkSavegameDirectory() => UnlinkDirectory(SavegameDirectory);

        /// <summary>
        /// Links this instances scenario directory to another location.
        /// All contents in the old directory will be deleted!
        /// </summary>
        /// <param name="destination">The location to link to.</param>
        public void LinkScenarioDirectory(string destination) => LinkDirectory(ScenarioDirectory, destination);

        /// <summary>
        /// Unlinks this instances scenario directory so it points to its original location.
        /// </summary>
        public void UnlinkScenarioDirectory() => UnlinkDirectory(ScenarioDirectory);

        /// <summary>
        /// Links this instances mod directory to another location.
        /// All contents in the old directory will be deleted!
        /// </summary>
        /// <param name="destination">The location to link to.</param>
        public void LinkModDirectory(string destination) => LinkDirectory(ModDirectory, destination);

        /// <summary>
        /// Unlinks this instances mod directory so it points to its original location.
        /// </summary>
        public void UnlinkModDirectory() => UnlinkDirectory(ModDirectory);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
