namespace ModMyFactory.ModSettings
{
    /// <summary>
    /// BAse class for all mod settings.
    /// </summary>
    public abstract class Setting<T> : ISetting<T>
    {
        /// <summary>
        /// The settings type.
        /// </summary>
        public abstract SettingType Type { get; }

        /// <summary>
        /// The settings runtime type.
        /// </summary>
        public RuntimeType RuntimeType { get; }

        /// <summary>
        /// The settings name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The display name of the setting, in Factorios localized string format.
        /// </summary>
        public string LocalisedName { get; }

        /// <summary>
        /// The description of the setting, in Factorios localized string format.
        /// </summary>
        public string LocalisedDescription { get; }

        /// <summary>
        /// A string used to determine the order of multiple settings.
        /// Order is determined by lexicographical comparison.
        /// </summary>
        public string Order { get; }

        /// <summary>
        /// The current value of the setting.
        /// </summary>
        public abstract T Value { get; set; }

        /// <summary>
        /// The default value of the setting.
        /// </summary>
        public T DefaultValue { get; }

        object ISetting.Value
        {
            get => Value;
            set => Value = (T)value;
        }

        object ISetting.DefaultValue => DefaultValue;

        protected Setting(RuntimeType runtimeType, string name, string localisedName, string localisedDescription, string order, T defaultValue)
            => (RuntimeType, Name, LocalisedName, LocalisedDescription, Order, DefaultValue)
               = (runtimeType, name, localisedName, localisedDescription, order, defaultValue);
    }
}
