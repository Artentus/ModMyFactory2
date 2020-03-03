//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModMyFactory.ModSettings
{
    /// <summary>
    /// A setting storing a string value.
    /// </summary>
    public sealed class StringSetting : Setting<string>
    {
        string _value;

        public override SettingType Type => SettingType.String;

        /// <summary>
        /// The current value of the setting.
        /// </summary>
        public override string Value
        {
            get => _value;
            set
            {
                if (AllowedValues is null)
                {
                    if (!AllowBlank && string.IsNullOrEmpty(value))
                        throw new ArgumentException("Setting does not allow empty values.");
                }
                else
                {
                    if (!AllowedValues.Contains(value))
                        throw new ArgumentException("Setting does not allow this value.");
                }
                
                _value = value;
            }
        }

        /// <summary>
        /// Indicates wether this setting allows for blank string values.
        /// Only has meaning if the allowed values list is empty.
        /// </summary>
        public bool AllowBlank { get; }

        /// <summary>
        /// A list of allowed values for this setting.
        /// If empty manual inputs are allowed.
        /// </summary>
        public IReadOnlyList<string> AllowedValues { get; }

        public StringSetting(RuntimeType runtimeType, string name, string localisedName, string localisedDescription, string order,
            string defaultValue, bool allowBlank = false)
            : base(runtimeType, name, localisedName, localisedDescription, order, defaultValue)
            => (AllowBlank, AllowedValues, _value) = (allowBlank, null, defaultValue);

        public StringSetting(RuntimeType runtimeType, string name, string localisedName, string localisedDescription, string order,
            string defaultValue, IList<string> allowedValues)
            : base(runtimeType, name, localisedName, localisedDescription, order, defaultValue)
        {
            AllowBlank = true;
            AllowedValues = new ReadOnlyCollection<string>(allowedValues);
            _value = defaultValue;
        }
    }
}
