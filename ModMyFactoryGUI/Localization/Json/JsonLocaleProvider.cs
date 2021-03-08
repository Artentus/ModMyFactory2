//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace ModMyFactoryGUI.Localization.Json
{
    internal sealed class JsonLocaleProvider : ILocaleProvider
    {
        private readonly IDictionary<string, ILocale> _locales;

        public ILocale this[string key] => _locales[key];
        public IEnumerable<string> Cultures => Keys;
        public IEnumerable<string> Keys => _locales.Keys;
        public IEnumerable<ILocale> Values => _locales.Values;
        public int Count => _locales.Count;

        public JsonLocaleProvider(DirectoryInfo directory)
        {
            _locales = new Dictionary<string, ILocale>();
            foreach (var jsonFile in directory.EnumerateFiles("*.json"))
            {
                if (IsValidCulture(Path.GetFileNameWithoutExtension(jsonFile.Name), out var culture))
                {
                    var locale = new JsonLocale(jsonFile, culture.Name);
                    _locales.Add(locale.Culture, locale);
                }
            }
        }

        private static bool IsValidCulture(string name, [NotNullWhen(true)] out CultureInfo? culture)
        {
            try
            {
                culture = CultureInfo.GetCultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                culture = null;
                return false;
            }

            return true;
        }

        public bool ContainsKey(string key) => _locales.ContainsKey(key);

        public bool TryGetValue(string key, [NotNullWhen(true)] out ILocale? value) => _locales.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, ILocale>> GetEnumerator() => _locales.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_locales).GetEnumerator();
    }
}
