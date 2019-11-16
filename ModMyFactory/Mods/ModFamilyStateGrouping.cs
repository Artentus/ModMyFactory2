using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// Groups information about the state of multiple mod families
    /// </summary>
    public sealed class ModFamilyStateGrouping
    {
        [JsonProperty("mods")]
        public IEnumerable<ModFamilyStateInfo> States { get; }

        [JsonConstructor]
        private ModFamilyStateGrouping(IEnumerable<ModFamilyStateInfo> states)
        {
            States = states;
        }

        /// <summary>
        /// Creates a JSON string from this state grouping.
        /// </summary>
        public string ToJson(Formatting formatting = Formatting.Indented, JsonSerializerSettings settings = null)
            => JsonConvert.SerializeObject(this, formatting, settings);

        /// <summary>
        /// Saves all state information to a file.
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
        /// Saves all state information to a file.
        /// </summary>
        public async Task SaveToFileAsync(string fileName, Formatting formatting = Formatting.Indented, JsonSerializerSettings settings = null)
            => await SaveToFileAsync(new FileInfo(fileName), formatting, settings);


        /// <summary>
        /// Creates and groups state information of all families in a mod manager.
        /// </summary>
        public static ModFamilyStateGrouping FromManager(ModManager manager)
        {
            var states = new List<ModFamilyStateInfo>(manager.Families.Count);
            foreach (var family in manager.Families)
                states.Add(ModFamilyStateInfo.FromFamily(family));
            return new ModFamilyStateGrouping(states);
        }

        /// <summary>
        /// Creates state information from a JSON string.
        /// </summary>
        public static ModFamilyStateGrouping FromJson(string json)
            => JsonConvert.DeserializeObject<ModFamilyStateGrouping>(json);

        /// <summary>
        /// Loads state information from a file.
        /// </summary>
        public static async Task<ModFamilyStateGrouping> FromFileAsync(FileInfo file)
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
        /// Loads state information from a file.
        /// </summary>
        public static async Task<ModFamilyStateGrouping> FromFileAsync(string fileName)
            => await FromFileAsync(new FileInfo(fileName));
    }
}
