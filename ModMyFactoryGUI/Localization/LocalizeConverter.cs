using System;
using System.Globalization;

namespace ModMyFactoryGUI.Localization
{
    sealed class LocalizeConverter : ValueConverterBase<string, string, object>
    {
        protected override string Convert(string value, object parameter, CultureInfo culture)
            => (string)App.Current.LocaleManager.GetResource(value);

        protected override string ConvertBack(string value, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
