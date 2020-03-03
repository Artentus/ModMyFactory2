//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

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
