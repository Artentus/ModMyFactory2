using System.Collections.Generic;

namespace ModMyFactoryGUI.Localization
{
    interface ILocaleProvider : IReadOnlyDictionary<string, ILocale>
    {
        IEnumerable<string> Cultures { get; }
    }
}
