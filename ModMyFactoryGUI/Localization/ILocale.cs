using System.Collections.Generic;

namespace ModMyFactoryGUI.Localization
{
    interface ILocale : IReadOnlyDictionary<string, object>
    {
        string Culture { get; }
    }
}
