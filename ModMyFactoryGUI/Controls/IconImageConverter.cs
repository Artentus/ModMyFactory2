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
    class IconImageConverter : IValueConverter
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
