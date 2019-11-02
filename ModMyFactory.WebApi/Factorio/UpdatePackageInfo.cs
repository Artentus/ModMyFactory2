using ModMyFactory.BaseTypes;
using Newtonsoft.Json;

namespace ModMyFactory.WebApi.Factorio
{
    /// <summary>
    /// Contains information about an update package.
    /// </summary>
    public struct UpdatePackageInfo
    {
        /// <summary>
        /// Version updating from.
        /// </summary>
        [JsonProperty("from")]
        public AccurateVersion From;

        /// <summary>
        /// Version updating to.
        /// </summary>
        [JsonProperty("to")]
        public AccurateVersion To;
    }
}
