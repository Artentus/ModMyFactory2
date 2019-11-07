using ModMyFactory.Mods;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.Game
{
    public static class Factorio
    {
        static async Task<(bool, IModFile)> TryLoadCoreModAsync(DirectoryInfo directory)
        {
            var coreModPath = Path.Combine(directory.FullName, "data", "core");
            return await ExtractedModFile.TryLoadAsync(coreModPath);
        }

        static async Task<(bool, IModFile)> TryLoadBaseModAsync(DirectoryInfo directory)
        {
            var baseModPath = Path.Combine(directory.FullName, "data", "base");
            return await ExtractedModFile.TryLoadAsync(baseModPath);
        }

        static bool TryLoadExecutable(DirectoryInfo directory, out FileInfo executable)
        {
#if NETFULL
            executable = new FileInfo(Path.Combine(directory.FullName, "bin", "x64", "factorio.exe"));
            return executable.Exists;
#elif NETCORE
            var os = Environment.OSVersion;
            executable = null;
            if (os.Platform == PlatformID.Win32NT)
                executable = new FileInfo(Path.Combine(directory.FullName, "bin", "x64", "factorio.exe"));
            else if (os.Platform == PlatformID.Unix)
                executable = new FileInfo(Path.Combine(directory.FullName, "bin", "x64", "factorio"));
            return (executable != null) && executable.Exists;
#else
            executable = null;
            return false;
#endif
        }

        /// <summary>
        /// Tries to load a Factorio instance.
        /// </summary>
        /// <param name="directory">The directory the instance is stored in.</param>
        public static async Task<(bool, FactorioInstance)> TryLoadAsync(DirectoryInfo directory)
        {
            if (!directory.Exists) return (false, null);
            if (!TryLoadExecutable(directory, out var executable)) return (false, null);

            (bool s1, var coreMod) = await TryLoadCoreModAsync(directory);
            if (!s1) return (false, null);

            (bool s2, var baseMod) = await TryLoadBaseModAsync(directory);
            if (!s2) return (false, null);

            return (true, new FactorioInstance(directory, executable, coreMod, baseMod));
        }

        /// <summary>
        /// Tries to load a Factorio instance.
        /// </summary>
        /// <param name="directory">The path the instance is stored at.</param>
        public static async Task<(bool, FactorioInstance)> TryLoadAsync(string path)
        {
            var dir = new DirectoryInfo(path);
            return await TryLoadAsync(dir);
        }

        /// <summary>
        /// Loads a Factorio instance.
        /// </summary>
        /// <param name="directory">The directory the instance is stored in.</param>
        public static async Task<FactorioInstance> LoadAsync(DirectoryInfo directory)
        {
            (bool success, var result) = await TryLoadAsync(directory);
            if (!success) throw new InvalidPathException("The directory does not contain a valid Factorio instance.");
            return result;
        }

        /// <summary>
        /// Loads a Factorio instance.
        /// </summary>
        /// <param name="directory">The path the instance is stored at.</param>
        public static async Task<FactorioInstance> LoadAsync(string path)
        {
            var dir = new DirectoryInfo(path);
            return await LoadAsync(dir);
        }
    }
}
