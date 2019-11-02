namespace ModMyFactory.ModSettings
{
    /// <summary>
    /// A setting storing a boolean value.
    /// </summary>
    public sealed class BooleanSetting : Setting<bool>
    {
        public override SettingType Type => SettingType.Boolean;

        /// <summary>
        /// The current value of the setting.
        /// </summary>
        public override bool Value { get; set; }

        public BooleanSetting(RuntimeType runtimeType, string name, string localisedName, string localisedDescription, string order, bool defaultValue)
            : base(runtimeType, name, localisedName, localisedDescription, order, defaultValue)
        {
            Value = defaultValue;
        }
    }
}
