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
        double _value;

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
        {
            MinValue = minValue;
            MaxValue = maxValue;
            AllowedValues = null;
            _value = defaultValue;
        }

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
