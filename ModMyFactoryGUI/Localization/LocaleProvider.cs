using ModMyFactoryGUI.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactoryGUI.Localization
{
    static class LocaleProvider
    {
        public const string DefaultCulture = "en";

        class EmptyLocale : ILocale
        {
            public string Culture => DefaultCulture;

            public object this[string key] => $"Missing key '{key}'";
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

        class EmptyLocaleProvider : ILocaleProvider
        {
            readonly ILocale _locale;

            public IEnumerable<string> Cultures => Keys;

            public ILocale this[string key] => _locale;
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


        // Fallback value if no locales are found for some reason.
        public static readonly ILocaleProvider Empty;

        static LocaleProvider()
        {
            Empty = new EmptyLocaleProvider();
        }
    }
}
