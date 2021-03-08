//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.ModSettings
{
    /// <summary>
    /// A mod setting.
    /// </summary>
    public interface ISetting
    {
        /// <summary>
        /// The settings type.
        /// </summary>
        SettingType Type { get; }

        /// <summary>
        /// The settings runtime type.
        /// </summary>
        RuntimeType RuntimeType { get; }

        /// <summary>
        /// The settings name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The display name of the setting, in Factorios localized string format.
        /// </summary>
        string LocalisedName { get; }

        /// <summary>
        /// The description of the setting, in Factorios localized string format.
        /// </summary>
        string LocalisedDescription { get; }

        /// <summary>
        /// A string used to determine the order of multiple settings.
        /// Order is determined by lexicographical comparison.
        /// </summary>
        string Order { get; }

        /// <summary>
        /// The current value of the setting.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// The default value of the setting.
        /// </summary>
        object DefaultValue { get; }
    }
}
