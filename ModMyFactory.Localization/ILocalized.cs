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
