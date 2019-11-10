using ModMyFactory.Mods;
using System.Diagnostics;
using System.IO;

namespace ModMyFactory.Game
{
    sealed class FactorioStandaloneInstance : FactorioInstance
    {
        readonly FileInfo _executable;

        internal FactorioStandaloneInstance(DirectoryInfo directory, IModFile coreMod, IModFile baseMod, FileInfo executable)
            : base(directory, coreMod, baseMod,
                  new DirectoryInfo(Path.Combine(directory.FullName, "saves")),
                  new DirectoryInfo(Path.Combine(directory.FullName, "scenarios")),
                  new DirectoryInfo(Path.Combine(directory.FullName, "mods")))
        {
            _executable = executable;
        }

        public override void Start(params string[] args)
        {
            var startInfo = new ProcessStartInfo(_executable.FullName, string.Join(" ", args));
            Process.Start(startInfo);
        }
    }
}
