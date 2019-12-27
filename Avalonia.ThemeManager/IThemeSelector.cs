// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license.
// Edited for ModMyFactory by Mathis Rech.
using System.Collections.Generic;
using Avalonia.Controls;
using ReactiveUI;

namespace Avalonia.ThemeManager
{
    /// <summary>
    /// Manages themes for windows.
    /// </summary>
    public interface IThemeSelector : IReactiveObject, ICollection<ITheme>
    {
        /// <summary>
        /// The currently selected theme.
        /// </summary>
        ITheme SelectedTheme { get; set; }

        /// <summary>
        /// The windows being managed.
        /// </summary>
        IReadOnlyList<Window> Windows { get; }

        /// <summary>
        /// Enables theme management on a window.
        /// </summary>
        void EnableThemes(Window window);

        /// <summary>
        /// Disables theme management on a window.
        /// </summary>
        bool DisableThemes(Window window);

        /// <summary>
        /// Selects a theme by name.
        /// </summary>
        bool SelectTheme(string name);

        /// <summary>
        /// Executes <see cref="SelectTheme"/>, useful for command bindings.
        /// </summary>
        ReactiveCommand<string, bool> SelectThemeCommand { get; }
    }
}
