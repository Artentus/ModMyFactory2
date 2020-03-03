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
    /// A Factorio instance that is managed by ModMyFactory.
    /// </summary>
    public sealed class ManagedFactorioInstance : IFactorioInstance
    {
        private readonly IFactorioInstance _baseInstance;

        internal bool HasManagerAttached { get; set; }
        public DirectoryInfo Directory => _baseInstance.Directory;

        public IModFile CoreMod => _baseInstance.CoreMod;

        public IModFile BaseMod => _baseInstance.BaseMod;

        public AccurateVersion Version => _baseInstance.Version;

        public DirectoryInfo SavegameDirectory => _baseInstance.SavegameDirectory;

        public DirectoryInfo ScenarioDirectory => _baseInstance.ScenarioDirectory;

        public DirectoryInfo ModDirectory => _baseInstance.ModDirectory;

        private ManagedFactorioInstance(IFactorioInstance baseInstance)
        {
            _baseInstance = baseInstance;
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

        internal static ManagedFactorioInstance FromInstance(IFactorioInstance instance)
        {
            if (instance is ManagedFactorioInstance)
                return (ManagedFactorioInstance)instance;
            return new ManagedFactorioInstance(instance);
        }

        internal void LinkModDirectoryInternal(string destination) => LinkDirectory(ModDirectory, destination);

        internal void UnlinkModDirectoryInternal() => UnlinkDirectory(ModDirectory);

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
        public void LinkModDirectory(string destination)
        {
            if (HasManagerAttached)
                throw new InvalidOperationException("Mod directory cannot be linked manually while the instance is attached to a manager.");
            LinkModDirectoryInternal(destination);
        }

        /// <summary>
        /// Unlinks this instances mod directory so it points to its original location.
        /// </summary>
        public void UnlinkModDirectory()
        {
            if (HasManagerAttached)
                throw new InvalidOperationException("Mod directory cannot be unlinked while the instance is attached to a manager.");
            UnlinkModDirectoryInternal();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
