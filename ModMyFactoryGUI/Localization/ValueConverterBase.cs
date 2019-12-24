using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ModMyFactoryGUI.Localization
{
    abstract class ValueConverterBase<TSource, TTarget, TParameter> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Convert((TSource)value, (TParameter)parameter, culture);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ConvertBack((TTarget)value, (TParameter)parameter, culture);

        protected abstract TTarget Convert(TSource value, TParameter parameter, CultureInfo culture);

        protected abstract TSource ConvertBack(TTarget value, TParameter parameter, CultureInfo culture);
    }
}
