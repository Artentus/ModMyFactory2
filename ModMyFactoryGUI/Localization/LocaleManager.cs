//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ModMyFactoryGUI.Localization
{
    sealed class LocaleManager
    {
        readonly ILocaleProvider _localeProvider;
        readonly ILocale _defaultLocale;
        CultureInfo _uiCulture;

        public event EventHandler UICultureChanged;

        public CultureInfo UICulture
        {
            get => _uiCulture;
            set
            {
                _uiCulture = value;
                UICultureChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public CultureInfo DefaultCulture { get; }

        public IEnumerable<CultureInfo> AvailableCultures => _localeProvider.Cultures.Select(s => CultureInfo.GetCultureInfo(s));

        public LocaleManager(ILocaleProvider localeProvider, CultureInfo defaultCulture)
        {
            _localeProvider = localeProvider;
            DefaultCulture = defaultCulture;
            _uiCulture = defaultCulture;

            string defaultCode = defaultCulture.TwoLetterISOLanguageName;
            if (!localeProvider.TryGetValue(defaultCode, out _defaultLocale))
                throw new LocaleException($"Default culture '{defaultCode}' not provided.");
        }

        public LocaleManager(ILocaleProvider localeProvider)
            : this(localeProvider, CultureInfo.GetCultureInfo(LocaleProvider.DefaultCulture))
        { }

        public LocaleManager()
            : this(LocaleProvider.Empty)
        { }

        object GetDefaultResource(string key)
        {
            if (_defaultLocale.TryGetValue(key, out var resource)) return resource;
            else return $"Missing key '{key}'";
        }

        object GetResource(ILocale locale, string key)
        {
            if (locale.TryGetValue(key, out var resource)) return resource;
            else return GetDefaultResource(key);
        }

        public object GetResource(string key)
        {
            if (_localeProvider.TryGetValue(UICulture.TwoLetterISOLanguageName, out var locale))
                return GetResource(locale, key);
            else
                return GetDefaultResource(key);
        }
    }
}
