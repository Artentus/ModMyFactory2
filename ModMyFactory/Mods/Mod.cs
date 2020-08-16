//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// A mod
    /// </summary>
    public class Mod : ICanEnable, IDisposable
    {
        private readonly IModFile _file;
        private bool _enabled;

        /// <summary>
        /// Is raised if the enabled state of this mod changes
        /// </summary>
        public event EventHandler EnabledChanged;

        /// <summary>
        /// The mod file
        /// </summary>
        public IModFile File => _file;

        /// <summary>
        /// The unique name of the mod
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The mods display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The mods version
        /// </summary>
        public AccurateVersion Version { get; }

        /// <summary>
        /// The version of Factorio this mod works on<br/>
        /// Only considers the major version, where version 1.0 is considered to be version 0.18 for compatibility reasons
        /// </summary>
        public AccurateVersion FactorioVersion { get; }

        /// <summary>
        /// The author of the mod
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// A description of the mod
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The dependencies of this mod
        /// </summary>
        public Dependency[] Dependencies { get; }

        /// <summary>
        /// A stream containing bitmap data of the mods thumbnail<br/>
        /// May be null
        /// </summary>
        public Stream Thumbnail { get; }

        /// <summary>
        /// Specifies whether this mod can be disabled
        /// </summary>
        public virtual bool CanDisable => true;

        /// <summary>
        /// Gets or sets if this mod is enabled<br/>
        /// If a mod is enabled all other mods in the same family will be disabled
        /// </summary>
        public bool Enabled
        {
            get => !CanDisable || _enabled;
            set
            {
                if (value == _enabled) return;
                if (!value && !CanDisable)
                    throw new InvalidOperationException("This mod cannot be disabled.");

                _enabled = value;
                if (!(_file is null) && (FactorioVersion < ModManager.FormatSwitch)) // Below 0.17 file needs to be disabled manually
                {
                    if (value) // Always enable file
                        _file.Enabled = true;
                    else if (!(Family is null) && (Family.Count > 1)) // Only disable file if there are other mods in the family
                        _file.Enabled = false;
                }
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The family this mod is part of<br/>
        /// Null if the mod has not been added to a family
        /// </summary>
        public ModFamily Family { get; internal set; }

        bool? ICanEnable.Enabled
        {
            get => Enabled;
            set
            {
                if (value is null) throw new InvalidOperationException("Cannot set enabled state to null");
                else Enabled = value.Value;
            }
        }

        protected Mod(string name, string displayName, AccurateVersion version, AccurateVersion factorioVersion,
            string author, string description, Dependency[] dependencies, Stream thumbnail = null)
            => (Name, DisplayName, Version, FactorioVersion, Author, Description, Dependencies, Thumbnail)
               = (name, displayName, version, factorioVersion, author, description, dependencies, thumbnail);

        protected Mod(ModInfo info, Stream Thumbnail = null)
            : this(info.Name, info.DisplayName, info.Version, info.FactorioVersion,
                  info.Author, info.Description, info.Dependencies, Thumbnail)
        { }

        public Mod(IModFile file)
            : this(file.Info, file.Thumbnail)
        {
            _file = file;
            Enabled = _file.Enabled;
            if (_file.Info.FactorioVersion >= ModManager.FormatSwitch)
                _file.Enabled = true; // Starting with Factorio 0.17 mod files should always be enabled
        }

        ~Mod()
        {
            Dispose(false);
        }

        private static Mod FromFile(IModFile file)
        {
            if (file is null) return null;
            return new Mod(file);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _file?.Dispose();
        }

        /// <summary>
        /// Tries to load a mod
        /// </summary>
        /// <param name="path">The path to load the mod from</param>
        public static async Task<(bool, Mod)> TryLoadAsync(string path)
        {
            (bool success, var file) = await ModFile.TryLoadAsync(path);
            return (success, FromFile(file));
        }

        /// <summary>
        /// Tries to load a mod
        /// </summary>
        /// <param name="fileSystemInfo">The path to load the mod from</param>
        public static async Task<(bool, Mod)> TryLoadAsync(FileSystemInfo fileSystemInfo)
        {
            (bool success, var file) = await ModFile.TryLoadAsync(fileSystemInfo);
            return (success, FromFile(file));
        }

        /// <summary>
        /// Loads a mod
        /// </summary>
        /// <param name="path">The path to load the mod from</param>
        public static async Task<Mod> LoadAsync(string path)
        {
            var file = await ModFile.LoadAsync(path);
            return FromFile(file);
        }

        /// <summary>
        /// Loads a mod
        /// </summary>
        /// <param name="fileSystemInfo">The path to load the mod from</param>
        public static async Task<Mod> LoadAsync(FileSystemInfo fileSystemInfo)
        {
            var file = await ModFile.LoadAsync(fileSystemInfo);
            return FromFile(file);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
