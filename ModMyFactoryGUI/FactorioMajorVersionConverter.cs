//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Data.Converters;
using ModMyFactory.BaseTypes;
using System;
using System.Globalization;

namespace ModMyFactoryGUI
{
    internal sealed class FactorioMajorVersionConverter : IValueConverter
    {
        private const string AllDescKey = "AllVersionsDesc";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var version = (AccurateVersion)value;
            if (version == default) return (string)App.Current.Locales.GetResource(AllDescKey);
            if (version == (0, 18)) return "Factorio 1.0 (0.18)";
            return "Factorio " + version.ToString(2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
