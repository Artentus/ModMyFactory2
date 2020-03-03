//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

// ----------------------------------------------------------------------------------------------------------------------
// File taken from
// https://github.com/VitalElement/AvalonStudio.Shell/blob/master/src/AvalonStudio.Shell/Converters/IconImageConverter.cs
// ----------------------------------------------------------------------------------------------------------------------


using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using System.IO;

namespace ModMyFactoryGUI.Controls
{
    internal class IconImageConverter : IValueConverter
    {
        public static IconImageConverter Converter { get; } = new IconImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindowIcon)
            {
                Bitmap result = null;

                using (var stream = new MemoryStream())
                {
                    (value as WindowIcon).Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    result = new Bitmap(stream);
                }

                return result;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
