using ModMyFactory.BaseTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModMyFactory.Mods
{
    public sealed class ModManager
    {
        public AccurateVersion FactorioVersion { get; }

        public DirectoryInfo Directory { get; }

        /// <param name="factorioVersion">The version of Factorio this mod manager targets. Only considers major version.</param>
        /// <param name="directory">The directory the managed mods reside in.</param>
        public ModManager(AccurateVersion factorioVersion, DirectoryInfo directory)
        {
            FactorioVersion = factorioVersion.ToMajor();
            Directory = directory;
        }
    }
}
