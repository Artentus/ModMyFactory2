//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using System.Collections.Generic;

namespace ModMyFactoryGUI.Controls
{
    internal interface IToolbarElement : IControl, IMenuElement
    {
        new IToolbarItem SelectedItem { get; set; }

        new IEnumerable<IToolbarItem> SubItems { get; }
    }
}
