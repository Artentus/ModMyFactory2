//  Copyright (C) 2020-2021 Mathis Rech
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
    /// A setting storing a floating point number value.
    /// </summary>
    public sealed class DoubleSetting : Setting<double>
    {
        private double _value;

        public override SettingType Type => SettingType.Double;

        /// <summary>
        /// The current value of the setting.
        /// </summary>
        public override double Value
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
        public double MinValue { get; }

        /// <summary>
        /// The maximum allowed value for this setting.
        /// Only has meaning of the allowed values list is empty.
        /// </summary>
        public double MaxValue { get; }

        /// <summary>
        /// A list of allowed values for this setting.
        /// If empty manual inputs are allowed.
        /// </summary>
        public IReadOnlyList<double> AllowedValues { get; }

        public DoubleSetting(RuntimeType runtimeType, string name, string localisedName, string localisedDescription, string order,
            double defaultValue, double minValue = double.NegativeInfinity, double maxValue = double.PositiveInfinity)
            : base(runtimeType, name, localisedName, localisedDescription, order, defaultValue)
            => (MinValue, MaxValue, AllowedValues, _value) = (minValue, maxValue, Array.Empty<double>(), defaultValue);

        public DoubleSetting(RuntimeType runtimeType, string name, string localisedName, string localisedDescription, string order,
            double defaultValue, IList<double> allowedValues)
            : base(runtimeType, name, localisedName, localisedDescription, order, defaultValue)
        {
            MinValue = double.NegativeInfinity;
            MaxValue = double.PositiveInfinity;
            AllowedValues = new ReadOnlyCollection<double>(allowedValues);
            _value = defaultValue;
        }
    }
}
