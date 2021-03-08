//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Globalization;

namespace ModMyFactory.Localization
{
    /// <summary>
    /// A localized object.
    /// </summary>
    public interface ILocalized
    {
        /// <summary>
        /// Gets this objects locale for a specific culture.
        /// If a locale for the desired culture is not available the locale of the default culture (en) is used.
        /// </summary>
        /// <param name="culture">The desired culture.</param>
        Locale GetLocale(CultureInfo culture);
    }
}
