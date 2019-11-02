using Newtonsoft.Json;

namespace ModMyFactory.ModSettings
{
    /// <summary>
    /// Defines when a mod setting is loaded.
    /// </summary>
    [JsonConverter(typeof(RuntimeTypeConverter))]
    public enum RuntimeType
    {
        /// <summary>
        /// The setting is loaded at game startup and can not be changed ingame.
        /// </summary>
        Startup,
        /// <summary>
        /// The setting affects all users in the game.
        /// </summary>
        Global,
        /// <summary>
        /// The setting only affects the local user.
        /// </summary>
        User
    }
}
