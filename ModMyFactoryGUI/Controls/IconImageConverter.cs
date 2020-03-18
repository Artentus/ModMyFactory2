//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using System.IO;

namespace ModMyFactoryGUI.Controls
{
    internal class IconImageConverter : ValueConverterBase<WindowIcon, IBitmap>
    {
        public static IconImageConverter Converter { get; } = new IconImageConverter();

        protected override IBitmap Convert(WindowIcon value, CultureInfo culture)
        {
            if (value is null) return null;

            using var stream = new MemoryStream();
            value.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new Bitmap(stream);
        }

        protected override WindowIcon ConvertBack(IBitmap value, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
