using Newtonsoft.Json;

namespace ModMyFactory.ModSettings
{
    /// <summary>
    /// The type of a mod setting.
    /// </summary>
    [JsonConverter(typeof(SettingTypeConverter))]
    public enum SettingType
    {
        Boolean,
        Integer,
        Double,
        String
    }
}
