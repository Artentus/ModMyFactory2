// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license.
// Edited for ModMyFactory by Mathis Rech.
using Avalonia.Styling;
using ReactiveUI;

namespace Avalonia.ThemeManager
{
    /// <summary>
    /// A visual theme.
    /// </summary>
    public interface ITheme : IReactiveObject
    {
        /// <summary>
        /// The themes name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The themes style.
        /// </summary>
        IStyle Style { get; }
    }
}
