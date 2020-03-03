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
    /// A setting storing an integer number value.
    /// </summary>
    public sealed class IntegerSetting : Setting<int>
    {
        int _value;

        public override SettingType Type => SettingType.Integer;

        /// <summary>
        /// The current value of the setting.
        /// </summary>
        public override int Value
        {
            get => _value;
            set
            {
                if (AllowedValues is null)
                {
                    if ((value < MinValue) || (value > MaxValue))
                        throw new ArgumentOutOfRangeException();
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
        /// The minimum allowed value for this setting.
        /// Only has meaning of the allowed values list is empty.
        /// </summary>
        public int MinValue { get; }

        /// <summary>
        /// The maximum allowed value for this setting.
        /// Only has meaning of the allowed values list is empty.
        /// </summary>
        public int MaxValue { get; }

        /// <summary>
        /// A list of allowed values for this setting.
        /// If empty manual inputs are allowed.
        /// </summary>
        public IReadOnlyList<int> AllowedValues { get; }

        public IntegerSetting(RuntimeType runtimeType, string name, string localisedName, string localisedDescription, string order,
            int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue)
            : base(runtimeType, name, localisedName, localisedDescription, order, defaultValue)
            => (MinValue, MaxValue, AllowedValues, _value) = (minValue, maxValue, null, defaultValue);

        public IntegerSetting(RuntimeType runtimeType, string name, string localisedName, string localisedDescription, string order,
            int defaultValue, IList<int> allowedValues)
            : base(runtimeType, name, localisedName, localisedDescription, order, defaultValue)
        {
            MinValue = int.MinValue;
            MaxValue = int.MaxValue;
            AllowedValues = new ReadOnlyCollection<int>(allowedValues);
            _value = defaultValue;
        }
    }
}
