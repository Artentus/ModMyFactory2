// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license.
// Edited for ModMyFactory by Mathis Rech.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
using ReactiveUI;

namespace Avalonia.ThemeManager
{
    /// <summary>
    /// Standard implementation of a theme selector.
    /// </summary>
    public class ThemeSelector : ReactiveObject, IThemeSelector
    {
        ITheme _selectedTheme;
        readonly IList<ITheme> _themes;
        readonly IList<Window> _windows;

        /// <summary>
        /// The currently selected theme.
        /// </summary>
        public ITheme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (value != _selectedTheme)
                {
                    if (!(_selectedTheme is null))
                    {
                        foreach (var window in _windows)
                            window.Styles.Remove(_selectedTheme.Style);
                    }

                    _selectedTheme = value;
                    this.RaisePropertyChanged(nameof(SelectedTheme));

                    if (!(_selectedTheme is null))
                    {
                        foreach (var window in _windows)
                            window.Styles.Add(_selectedTheme.Style);
                    }
                }
            }
        }

        /// <summary>
        /// The windows being managed.
        /// </summary>
        public IReadOnlyList<Window> Windows { get; }

        /// <summary>
        /// Executes <see cref="SelectTheme"/>, useful for command bindings.
        /// </summary>
        public ReactiveCommand<string, bool> SelectThemeCommand { get; }

        public int Count => _themes.Count;

        public bool IsReadOnly => false;

        public ThemeSelector(IEnumerable<ITheme> themes)
        {
            _themes = new List<ITheme>(themes);
            _windows = new List<Window>();
            Windows = new ReadOnlyCollection<Window>(_windows);
            SelectThemeCommand = ReactiveCommand.Create<string, bool>(SelectTheme);
        }

        public ThemeSelector(params ITheme[] themes)
            : this((IEnumerable<ITheme>)themes)
        { }

        public void Add(ITheme theme) => _themes.Add(theme);

        public void AddRange(IEnumerable<ITheme> themes)
        {
            foreach (var theme in themes)
                Add(theme);
        }

        public void AddRange(params ITheme[] themes)
        {
            foreach (var theme in themes)
                Add(theme);
        }

        public bool Remove(ITheme theme)
        {
            if (theme == SelectedTheme)
                SelectedTheme = null;

            return _themes.Remove(theme);
        }

        public void Clear()
        {
            _themes.Clear();
            SelectedTheme = null;
        }

        public bool Contains(ITheme item) => _themes.Contains(item);
        void ICollection<ITheme>.CopyTo(ITheme[] array, int arrayIndex) => _themes.CopyTo(array, arrayIndex);
        public IEnumerator<ITheme> GetEnumerator() => _themes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _themes.GetEnumerator();

        /// <summary>
        /// Enables theme management on a window.
        /// </summary>
        public void EnableThemes(Window window)
        {
            if (window is null)
                throw new ArgumentNullException(nameof(window));

            if (_windows.Contains(window)) return;

            _windows.Add(window);
            if (!(SelectedTheme is null))
                window.Styles.Add(SelectedTheme.Style);
        }

        /// <summary>
        /// Disables theme management on a window.
        /// </summary>
        public bool DisableThemes(Window window)
        {
            if (window is null) return false;

            bool result = _windows.Remove(window);
            if (result && !(SelectedTheme is null))
                window.Styles.Remove(SelectedTheme.Style);
            return result;
        }

        /// <summary>
        /// Selects a theme by name.
        /// </summary>
        public bool SelectTheme(string name)
        {
            var candidates = _themes.Where(t => t.Name == name);
            if (candidates.Any())
            {
                var theme = candidates.First();
                SelectedTheme = theme;
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Tries to load themes from XAML files in a directory and adds them to a selector.
        /// </summary>
        /// <param name="directory">The directory to look for XAML theme files in.</param>
        /// <param name="selector">Out. The selector containing the loaded themes.</param>
        /// <returns>Returns true if a selector could be created, otherwise false.</returns>
        public static bool TryLoad(DirectoryInfo directory, out IThemeSelector selector)
        {
            selector = default;
            if ((directory is null) || !directory.Exists) return false;

            var themes = new List<ITheme>();
            foreach (var file in directory.EnumerateFiles("*.xaml"))
            {
                if (Theme.TryLoad(file, out var theme) && !(theme is null))
                    themes.Add(theme);
            }

            selector = new ThemeSelector(themes);
            return true;
        }

        /// <summary>
        /// Tries to load themes from XAML files in a directory and adds them to a selector.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to look for XAML theme files in.</param>
        /// <param name="selector">Out. The selector containing the loaded themes.</param>
        /// <returns>Returns true if a selector could be created, otherwise false.</returns>
        public static bool TryLoad(string directoryPath, out IThemeSelector selector)
            => TryLoad(new DirectoryInfo(directoryPath), out selector);

        /// <summary>
        /// Loads themes from XAML files in a directory and adds them to a selector.
        /// </summary>
        /// <param name="directory">The directory to look for XAML theme files in.</param>
        /// <returns>Returns a selector containg the loaded themes.</returns>
        public static IThemeSelector Load(DirectoryInfo directory)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            var themes = new List<ITheme>();
            foreach (var file in directory.EnumerateFiles("*.xaml"))
            {
                if (Theme.TryLoad(file, out var theme))
                    themes.Add(theme);
            }

            return new ThemeSelector(themes);
        }

        /// <summary>
        /// Loads themes from XAML files in a directory and adds them to a selector.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to look for XAML theme files in.</param>
        /// <returns>Returns a selector containg the loaded themes.</returns>
        public static IThemeSelector Load(string directoryPath)
            => Load(new DirectoryInfo(directoryPath));

        /// <summary>
        /// Loads themes from XAML files in a directory and adds them to a selector.
        /// If no selector could be created or no themes could be loaded a default selector will be returned instead.
        /// </summary>
        /// <param name="directory">The directory to look for XAML theme files in.</param>
        /// <returns>Returns a selector containg the loaded themes.</returns>
        public static IThemeSelector LoadSafe(DirectoryInfo directory)
        {
            if (!TryLoad(directory, out var selector) || (selector.Count == 0))
                selector = new ThemeSelector(Theme.DefaultLight, Theme.DefaultDark);
            return selector;
        }

        /// <summary>
        /// Loads themes from XAML files in a directory and adds them to a selector.
        /// If no selector could be created or no themes could be loaded a default selector will be returned instead.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to look for XAML theme files in.</param>
        /// <returns>Returns a selector containg the loaded themes.</returns>
        public static IThemeSelector LoadSafe(string directoryPath)
            => LoadSafe(new DirectoryInfo(directoryPath));
    }
}
