namespace ModMyFactory.ModSettings
{
    /// <summary>
    /// A mod setting, strongly typed.
    /// </summary>
    public interface ISetting<T> : ISetting
    {
        /// <summary>
        /// The current value of the setting.
        /// </summary>
        new T Value { get; set; }

        /// <summary>
        /// The default value of the setting.
        /// </summary>
        new T DefaultValue { get; }
    }
}
