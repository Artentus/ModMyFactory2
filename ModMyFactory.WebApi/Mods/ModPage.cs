using Newtonsoft.Json;

namespace ModMyFactory.WebApi.Mods
{
    /// <summary>
    /// AN API page containing a list of mods.
    /// </summary>
    public struct ModPage
    {
        /// <summary>
        /// Information about the page.
        /// </summary>
        [JsonProperty("pagination")]
        readonly public Pagination Pagination;

        /// <summary>
        /// A list of mods.
        /// </summary>
        [JsonProperty("results")]
        readonly public ModInfo[] Mods;

        [JsonConstructor]
        internal ModPage(Pagination pagination, ModInfo[] mods)
        {
            Pagination = pagination;
            Mods = mods;
        }
    }
}
