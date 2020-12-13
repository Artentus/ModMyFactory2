//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ModMyFactoryGUI
{
    internal abstract class ValueConverterBase<TSource, TTarget, TParameter> : IValueConverter
    {
        protected abstract TTarget Convert(TSource value, TParameter parameter, CultureInfo culture);

        protected abstract TSource ConvertBack(TTarget value, TParameter parameter, CultureInfo culture);

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert((TSource)value, (TParameter)parameter, culture);

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => ConvertBack((TTarget)value, (TParameter)parameter, culture);
    }

    internal abstract class ValueConverterBase<TSource, TTarget> : IValueConverter
    {
        protected abstract TTarget Convert(TSource value, CultureInfo culture);

        protected abstract TSource ConvertBack(TTarget value, CultureInfo culture);

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert((TSource)value, culture);

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => ConvertBack((TTarget)value, culture);
    }
}
