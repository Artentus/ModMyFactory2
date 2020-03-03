//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Controls.Generators;

namespace ModMyFactoryGUI.Controls
{
    internal class ToolbarItemContainerGenerator : ItemContainerGenerator<ToolbarItem>
    {
        public ToolbarItemContainerGenerator(IControl owner)
            : base(owner, ToolbarItem.HeaderProperty, null)
        { }

        protected override IControl CreateContainer(object item)
            => item as Separator ?? base.CreateContainer(item);
    }
}
