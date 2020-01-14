using System.Diagnostics;
using System.Reflection;

namespace ModMyFactoryGUI.Helpers
{
    static class AssemblyExtensions
    {
        public static FileVersionInfo FileVersion(this Assembly assembly)
            => FileVersionInfo.GetVersionInfo(assembly.Location);

        public static string ProductVersion(this Assembly assembly)
            => assembly.FileVersion().ProductVersion;
    }
}
