using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// Information about a mod.
    /// </summary>
    public struct ModInfo
    {
        /// <summary>
        /// The unique name of the mod.
        /// </summary>
        [JsonProperty("name")]
        readonly public string Name;

        /// <summary>
        /// The mods display name.
        /// </summary>
        [JsonProperty("title")]
        readonly public string DisplayName;

        /// <summary>
        /// The mods version.
        /// </summary>
        [JsonProperty("version")]
        readonly public AccurateVersion Version;

        /// <summary>
        /// The version of Factorio this mod works on.
        /// </summary>
        [JsonProperty("factorio_version")]
        readonly public AccurateVersion FactorioVersion;

        /// <summary>
        /// The author of the mod.
        /// </summary>
        [JsonProperty("author")]
        readonly public string Author;

        /// <summary>
        /// A description of the mod.
        /// </summary>
        [JsonProperty("description")]
        readonly public string Description;

        /// <summary>
        /// The dependencies of this mod.
        /// </summary>
        [JsonProperty("dependencies")]
        [JsonConverter(typeof(SingleOrArrayConverter<Dependency>))]
        readonly public Dependency[] Dependencies;

        [JsonConstructor]
        internal ModInfo(string name, string displayName, AccurateVersion version, AccurateVersion factorioVersion, string author, string description, params Dependency[] dependencies)
        {
            Name = name;
            DisplayName = displayName;
            Version = version;
            FactorioVersion = factorioVersion;
            Author = author;
            Description = description;
            Dependencies = dependencies;
        }

        /// <summary>
        /// Creates a json string from this mod info.
        /// </summary>
        public string ToJson(Formatting formatting = Formatting.Indented, JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(this, formatting, settings);
        }

        /// <summary>
        /// Saves the mod info file.
        /// </summary>
        public async Task SaveToFileAsync(FileInfo file, Formatting formatting = Formatting.Indented, JsonSerializerSettings settings = null)
        {
            if (!file.Directory.Exists) file.Directory.Create();
            using (var fs = file.OpenWrite())
            {
                using (var writer = new StreamWriter(fs))
                {
                    string json = ToJson(formatting, settings);
                    await writer.WriteAsync(json);
                }
            }
        }

        /// <summary>
        /// Saves the mod info file.
        /// </summary>
        public async Task SaveToFileAsync(string fileName, Formatting formatting = Formatting.Indented, JsonSerializerSettings settings = null)
            => await SaveToFileAsync(new FileInfo(fileName), formatting, settings);

        /// <summary>
        /// Loads mod info from a json string.
        /// </summary>
        public static ModInfo FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ModInfo>(json);
        }

        /// <summary>
        /// Loads a mod info file.
        /// </summary>
        public static async Task<ModInfo> FromFileAsync(FileInfo file)
        {
            using (var fs = file.OpenRead())
            {
                using (var reader = new StreamReader(fs))
                {
                    string json = await reader.ReadToEndAsync();
                    return FromJson(json);
                }
            }
        }

        /// <summary>
        /// Loads a mod info file.
        /// </summary>
        public static async Task<ModInfo> FromFileAsync(string fileName)
            => await FromFileAsync(new FileInfo(fileName));
    }
}
