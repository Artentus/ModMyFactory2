using ModMyFactory.BaseTypes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// A mod.
    /// </summary>
    public class Mod : IDisposable
    {
        readonly IModFile _file;

        /// <summary>
        /// The unique name of the mod.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The mods display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The mods version.
        /// </summary>
        public AccurateVersion Version { get; }

        /// <summary>
        /// The version of Factorio this mod works on.
        /// Only considers the major version.
        /// </summary>
        public AccurateVersion FactorioVersion { get; }

        /// <summary>
        /// The author of the mod.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// A description of the mod.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The dependencies of this mod.
        /// </summary>
        public Dependency[] Dependencies { get; }

        /// <summary>
        /// A stream containing bitmap data of the mods thumbnail. Optional.
        /// </summary>
        public Stream Thumbnail { get; }

        /// <summary>
        /// Specifies whether this mod can be disabled.
        /// </summary>
        public virtual bool CanDisable => true;

        protected Mod(string name, string displayName, AccurateVersion version, AccurateVersion factorioVersion,
            string author, string description, Dependency[] dependencies, Stream thumbnail = null)
        {
            Name = name;
            DisplayName = displayName;
            FactorioVersion = factorioVersion.ToMajor();
            Author = author;
            Description = description;
            Dependencies = dependencies;
            Thumbnail = thumbnail;
        }

        protected Mod(ModInfo info, Stream Thumbnail = null)
            : this(info.Name, info.DisplayName, info.Version, info.FactorioVersion,
                  info.Author, info.Description, info.Dependencies, Thumbnail)
        { }

        public Mod(IModFile file)
            : this(file.Info, file.Thumbnail)
        {
            _file = file;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _file?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Mod()
        {
            Dispose(false);
        }


        static Mod FromFile(IModFile file)
        {
            if (file is null) return null;
            return new Mod(file);
        }

        /// <summary>
        /// Tries to load a mod.
        /// </summary>
        /// <param name="path">The path to load the mod from.</param>
        public static async Task<(bool, Mod)> TryLoadAsync(string path)
        {
            (bool success, var file) = await ModFile.TryLoadAsync(path);
            return (success, FromFile(file));
        }

        /// <summary>
        /// Tries to load a mod.
        /// </summary>
        /// <param name="fileSystemInfo">The path to load the mod from.</param>
        public static async Task<(bool, Mod)> TryLoadAsync(FileSystemInfo fileSystemInfo)
        {
            (bool success, var file) = await ModFile.TryLoadAsync(fileSystemInfo);
            return (success, FromFile(file));
        }

        /// <summary>
        /// Loads a mod.
        /// </summary>
        /// <param name="path">The path to load the mod from.</param>
        public static async Task<Mod> LoadAsync(string path)
        {
            var file = await ModFile.LoadAsync(path);
            return FromFile(file);
        }

        /// <summary>
        /// Loads a mod.
        /// </summary>
        /// <param name="fileSystemInfo">The path to load the mod from.</param>
        public static async Task<Mod> LoadAsync(FileSystemInfo fileSystemInfo)
        {
            var file = await ModFile.LoadAsync(fileSystemInfo);
            return FromFile(file);
        }
    }
}
