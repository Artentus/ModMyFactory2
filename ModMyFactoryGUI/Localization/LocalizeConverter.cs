//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Globalization;

namespace ModMyFactoryGUI.Localization
{
    sealed class LocalizeConverter : ValueConverterBase<string, string, object>
    {
        protected override string Convert(string value, object parameter, CultureInfo culture)
            => (string)App.Current.LocaleManager.GetResource(value);

        protected override string ConvertBack(string value, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
