using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using System;
using System.IO;

namespace ModMyFactory.Game
{
    abstract class FactorioInstanceBase : IFactorioInstance
    {
        public DirectoryInfo Directory { get; }

        public IModFile CoreMod { get; }

        public IModFile BaseMod { get; }

        public AccurateVersion Version => CoreMod.Info.Version;

        public DirectoryInfo SavegameDirectory { get; }

        public DirectoryInfo ScenarioDirectory { get; }

        public DirectoryInfo ModDirectory { get; }

        protected FactorioInstanceBase(DirectoryInfo directory, IModFile coreMod, IModFile baseMod,
            DirectoryInfo savegameDirectory, DirectoryInfo scenarioDirectory, DirectoryInfo modDirectory)
            => (Directory, CoreMod, BaseMod, SavegameDirectory, ScenarioDirectory, ModDirectory)
               = (directory, coreMod, baseMod, savegameDirectory, scenarioDirectory, modDirectory);

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

        ~FactorioInstanceBase()
        {
            Dispose(false);
        }
    }
}
