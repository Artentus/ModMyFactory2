using System.Globalization;
using System.Text.RegularExpressions;

namespace ModMyFactory.Localization
{
    /// <summary>
    /// Resolves localized strings like Factorio.
    /// </summary>
    public sealed class Locale
    {
        readonly IniFile _iniFile;

        /// <summary>
        /// The culture of this locale.
        /// </summary>
        public CultureInfo Culture { get; }

        public Locale(CultureInfo culture, IniFile iniFile)
        {
            Culture = culture;
            _iniFile = iniFile;
        }

        /// <summary>
        /// Tries to resolve a localized string.
        /// </summary>
        /// <param name="key">The key to resolve.</param>
        /// <param name="args">Optional. Arguments to be substituted into the string.</param>
        public bool TryResolve(string key, out string result, params object[] args)
        {
            var parts = key.Split(new[] { '.' }, 2);
            string section;
            string vKey;

            if (parts.Length == 1)
            {
                section = IniFile.EmptySection;
                vKey = parts[0];
            }
            else if (parts.Length > 1)
            {
                section = parts[0];
                vKey = parts[1];
            }
            else
            {
                result = null;
                return false;
            }

            if (!_iniFile.TryGetValue(section, vKey, out result)) return false;

            const string pattern = @"__(\d+)__";
            const string replacePattern = "{$1}";
            result = Regex.Replace(result, pattern, replacePattern);

            result = string.Format(result, args);
            return true;
        }
    }
}
