using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using System;
using System.IO;

namespace ModMyFactory.Game
{
    /// <summary>
    /// A Factorio instance.
    /// </summary>
    public abstract class FactorioInstance : IDisposable
    {
        /// <summary>
        /// The directory the instance is stored in.
        /// </summary>
        public DirectoryInfo Directory { get; }

        /// <summary>
        /// The Factorio core mod.
        /// </summary>
        public IModFile CoreMod { get; }

        /// <summary>
        /// The Factorio base mod.
        /// </summary>
        public IModFile BaseMod { get; }

        /// <summary>
        /// The version of the instance.
        /// </summary>
        public AccurateVersion Version => CoreMod.Info.Version;

        /// <summary>
        /// The directory this instance stores its save data in.
        /// </summary>
        public DirectoryInfo SavegameDirectory { get; }

        /// <summary>
        /// The directory this instance loads scenarios from.
        /// </summary>
        public DirectoryInfo ScenarioDirectory { get; }

        /// <summary>
        /// The directory this instance is loading mods from.
        /// </summary>
        public DirectoryInfo ModDirectory { get; }

        protected FactorioInstance(DirectoryInfo directory, IModFile coreMod, IModFile baseMod,
                                   DirectoryInfo savegameDirectory, DirectoryInfo scenarioDirectory, DirectoryInfo modDirectory)
        {
            Directory = directory;
            CoreMod = coreMod;
            BaseMod = baseMod;
            SavegameDirectory = savegameDirectory;
            ScenarioDirectory = scenarioDirectory;
            ModDirectory = modDirectory;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <param name="args">Optional command line arguments.</param>
        public abstract void Start(params string[] args);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CoreMod.Dispose();
                BaseMod.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FactorioInstance()
        {
            Dispose(false);
        }
    }
}
