// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license.
// Edited for ModMyFactory by Mathis Rech.

using System;
using System.IO;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using ReactiveUI;

namespace Avalonia.ThemeManager
{
    /// <summary>
    /// Standard implementation of a visual theme.
    /// </summary>
    public class Theme : ReactiveObject, ITheme
    {
        /// <summary>
        /// The themes name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The themes style.
        /// </summary>
        public IStyle Style { get; }

        protected Theme(string name, IStyle style)
        {
            Name = name;
            Style = style;
        }


        static readonly IStyle _light;
        static readonly IStyle _dark;

        /// <summary>
        /// The default light theme of Avalonia.
        /// </summary>
        public static ITheme DefaultLight => new Theme("Light", _light);

        /// <summary>
        /// The default dark theme of Avalonia.
        /// </summary>
        public static ITheme DefaultDark => new Theme("Dark", _dark);

        static Theme()
        {
            _light = new StyleInclude(new Uri("resm:Styles?assembly=Avalonia.ThemeManager"))
            {
                Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default")
            };
            _dark = new StyleInclude(new Uri("resm:Styles?assembly=Avalonia.ThemeManager"))
            {
                Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseDark.xaml?assembly=Avalonia.Themes.Default")
            };
        }

        /// <summary>
        /// Tries to load a theme from an XAML file.
        /// </summary>
        /// <param name="file">The XAML file containing the style definition.</param>
        /// <param name="theme">Out. The loaded theme or null if the file could not be loaded.</param>
        /// <returns>Returns true if the file was successfully loaded, otherwise false.</returns>
        public static bool TryLoad(FileInfo file, out ITheme theme)
        {
            theme = default;
            if ((file is null) || !file.Exists) return false;

            var name = Path.GetFileNameWithoutExtension(file.Name);
            var xaml = File.ReadAllText(file.FullName);

            IStyle style;
            try
            {
                style = AvaloniaXamlLoader.Parse<IStyle>(xaml);
            }
            catch
            {
                return false;
            }

            theme = new Theme(name, style);
            return true;
        }

        /// <summary>
        /// Tries to load a theme from an XAML file.
        /// </summary>
        /// <param name="filePath">Path to the XAML file containing the style definition.</param>
        /// <param name="theme">Out. The loaded theme or null if the file could not be loaded.</param>
        /// <returns>Returns true if the file was successfully loaded, otherwise false.</returns>
        public static bool TryLoad(string filePath, out ITheme theme)
            => TryLoad(new FileInfo(filePath), out theme);

        /// <summary>
        /// Loads a theme from an XAML file.
        /// </summary>
        /// <param name="file">The XAML file containing the style definition.</param>
        /// <returns>Returns the loaded theme.</returns>
        public static ITheme Load(FileInfo file)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            var name = Path.GetFileNameWithoutExtension(file.Name);
            var xaml = File.ReadAllText(file.FullName);
            var style = AvaloniaXamlLoader.Parse<IStyle>(xaml);
            return new Theme(name, style);
        }

        /// <summary>
        /// Loads a theme from an XAML file.
        /// </summary>
        /// <param name="filePath">Path to the XAML file containing the style definition.</param>
        /// <returns>Returns the loaded theme.</returns>
        public static ITheme Load(string filePath)
            => Load(new FileInfo(filePath));
    }
}
