//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Globalization;

namespace ModMyFactoryGUI.Localization
{
    internal sealed class LocalizedFormatter : ValueConverterBase<object, string, string>
    {
        protected override string Convert(object value, string parameter, CultureInfo culture)
        {
            var formatString = (string)App.Current.Locales.GetResource(parameter);
            return string.Format(formatString, value);
        }

        protected override object ConvertBack(string value, string parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
