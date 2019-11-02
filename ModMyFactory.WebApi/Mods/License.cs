using Newtonsoft.Json;

namespace ModMyFactory.WebApi.Mods
{
    /// <summary>
    /// Represents a software license.
    /// </summary>
    public struct License
    {
        /// <summary>
        /// The name of the license.
        /// </summary>
        [JsonProperty("name")]
        readonly public string Name;

        /// <summary>
        /// A URL to the license.
        /// </summary>
        [JsonProperty("url")]
        readonly public string Url;

        [JsonConstructor]
        internal License(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}
