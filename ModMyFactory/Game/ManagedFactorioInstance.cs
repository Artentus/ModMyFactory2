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
        readonly IFactorioInstance _baseInstance;

        public DirectoryInfo Directory => _baseInstance.Directory;

        public IModFile CoreMod => _baseInstance.CoreMod;

        public IModFile BaseMod => _baseInstance.BaseMod;

        public AccurateVersion Version => _baseInstance.Version;

        public DirectoryInfo SavegameDirectory => _baseInstance.SavegameDirectory;

        public DirectoryInfo ScenarioDirectory => _baseInstance.ScenarioDirectory;

        public DirectoryInfo ModDirectory => _baseInstance.ModDirectory;

        internal bool HasManagerAttached { get; set; }

        private ManagedFactorioInstance(IFactorioInstance baseInstance)
        {
            _baseInstance = baseInstance;
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

        internal void LinkModDirectoryInternal(string destination) => LinkDirectory(ModDirectory, destination);

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

        internal void UnlinkModDirectoryInternal() => UnlinkDirectory(ModDirectory);

        /// <summary>
        /// Unlinks this instances mod directory so it points to its original location.
        /// </summary>
        public void UnlinkModDirectory()
        {
            if (HasManagerAttached)
                throw new InvalidOperationException("Mod directory cannot be unlinked while the instance is attached to a manager.");
            UnlinkModDirectoryInternal();
        }

        void Dispose(bool disposing)
        {
            if (disposing)
                _baseInstance.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ManagedFactorioInstance()
        {
            Dispose(false);
        }


        internal static ManagedFactorioInstance FromInstance(IFactorioInstance instance)
        {
            if (instance is ManagedFactorioInstance)
                return (ManagedFactorioInstance)instance;
            return new ManagedFactorioInstance(instance);
        }

        static void LinkDirectory(DirectoryInfo directory, string destination)
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

        static void UnlinkDirectory(DirectoryInfo directory)
        {
            var link = Symlink.FromPath(directory.FullName);
            if (link.Exists) link.Delete();
        }
    }
}
