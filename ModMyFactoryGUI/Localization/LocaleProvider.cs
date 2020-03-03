//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactoryGUI.Localization
{
    internal static class LocaleProvider
    {
        private class EmptyLocale : ILocale
        {
            public object this[string key] => $"Missing key '{key}'";
            public string Culture => DefaultCulture;
            public IEnumerable<string> Keys => Enumerable.Empty<string>();
            public IEnumerable<object> Values => Enumerable.Empty<object>();
            public int Count => 0;

            public bool TryGetValue(string key, out object value)
            {
                value = this[key];
                return true;
            }

            public bool ContainsKey(string key) => true;

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class EmptyLocaleProvider : ILocaleProvider
        {
            private readonly ILocale _locale;

            public ILocale this[string key] => _locale;
            public IEnumerable<string> Cultures => Keys;
            public IEnumerable<string> Keys => DefaultCulture.ToEnumerable();
            public IEnumerable<ILocale> Values => _locale.ToEnumerable();
            public int Count => 1;

            public EmptyLocaleProvider()
            {
                _locale = new EmptyLocale();
            }

            public bool TryGetValue(string key, out ILocale value)
            {
                value = _locale;
                return true;
            }

            public bool ContainsKey(string key) => true;

            public IEnumerator<KeyValuePair<string, ILocale>> GetEnumerator() => (new KeyValuePair<string, ILocale>(DefaultCulture, _locale)).ToEnumerable().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public const string DefaultCulture = "en";

        // Fallback value if no locales are found for some reason.
        public static readonly ILocaleProvider Empty;

        static LocaleProvider()
        {
            Empty = new EmptyLocaleProvider();
        }
    }
}
