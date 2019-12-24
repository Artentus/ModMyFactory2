using System;
using System.Globalization;

namespace ModMyFactoryGUI.Localization
{
    sealed class LocalizedFormatter : ValueConverterBase<object, string, string>
    {
        protected override string Convert(object value, string parameter, CultureInfo culture)
        {
            var formatString = (string)App.Current.LocaleManager.GetResource(parameter);
            return string.Format(formatString, value);
        }

        protected override object ConvertBack(string value, string parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
