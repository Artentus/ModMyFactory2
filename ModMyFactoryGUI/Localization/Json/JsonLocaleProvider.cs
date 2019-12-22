using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ModMyFactoryGUI.Localization.Json
{
    sealed class JsonLocaleProvider : ILocaleProvider
    {
        readonly IDictionary<string, ILocale> _locales;

        public IEnumerable<string> Cultures => Keys;

        public ILocale this[string key] => _locales[key];
        public IEnumerable<string> Keys => _locales.Keys;
        public IEnumerable<ILocale> Values => _locales.Values;
        public int Count => _locales.Count;

        static bool IsValidCulture(string name, out CultureInfo culture)
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

        public JsonLocaleProvider(DirectoryInfo directory)
        {
            _locales = new Dictionary<string, ILocale>();
            foreach (var jsonFile in directory.EnumerateFiles("*.json"))
            {
                if (IsValidCulture(Path.GetFileNameWithoutExtension(jsonFile.Name), out var culture))
                {
                    var locale = new JsonLocale(jsonFile, culture.TwoLetterISOLanguageName);
                    _locales.Add(locale.Culture, locale);
                }
            }
        }

        public bool ContainsKey(string key) => _locales.ContainsKey(key);
        public bool TryGetValue(string key, out ILocale value) => _locales.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, ILocale>> GetEnumerator() => _locales.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_locales).GetEnumerator();
    }
}
