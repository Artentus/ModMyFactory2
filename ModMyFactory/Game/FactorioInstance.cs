using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using System;
using System.Diagnostics;
using System.IO;

namespace ModMyFactory.Game
{
    /// <summary>
    /// A Factorio instance.
    /// </summary>
    public class FactorioInstance : IDisposable
    {
        /// <summary>
        /// The directory the instance is stored in.
        /// </summary>
        public DirectoryInfo Directory { get; }

        /// <summary>
        /// The executable file of the instance.
        /// </summary>
        public FileInfo Executable { get; }

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

        internal FactorioInstance(DirectoryInfo directory, FileInfo executable, IModFile coreMod, IModFile baseMod)
        {
            Directory = directory;
            Executable = executable;
            CoreMod = coreMod;
            BaseMod = baseMod;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <param name="arguments">Optional command line arguments.</param>
        public void Start(params string[] arguments)
        {
            var startInfo = new ProcessStartInfo(Executable.FullName, string.Join(" ", arguments));
            Process.Start(startInfo);
        }

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
