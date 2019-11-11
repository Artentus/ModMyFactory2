using ModMyFactory.BaseTypes;
using ModMyFactory.Game;
using ModMyFactory.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

        public void LinkSavegameDirectory(string destination)
        {

        }

        public void UnlinkSavegameDirectory()
        {

        }

        public void LinkScenarioDirectory(string destination)
        {

        }

        public void UnlinkScenarioDirectory()
        {

        }

        internal void LinkModDirectoryInternal(string destination)
        {

        }

        public void LinkModDirectory(string destination)
        {
            if (HasManagerAttached)
                throw new InvalidOperationException("Mod directory cannot be linked manually while the instance is attached to a manager.");
            LinkModDirectoryInternal(destination);
        }

        internal void UnlinkModDirectoryInternal()
        {

        }

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
    }
}
