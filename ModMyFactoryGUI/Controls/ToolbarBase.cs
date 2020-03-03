//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Controls.Platform;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactoryGUI.Controls
{
    internal abstract class ToolbarBase : MenuBase, IToolbar
    {
        IToolbarItem IToolbarElement.SelectedItem
        {
            get
            {
                var index = SelectedIndex;
                return (index != -1)
                    ? (IToolbarItem)ItemContainerGenerator.ContainerFromIndex(index)
                    : null;
            }
            set => SelectedIndex = ItemContainerGenerator.IndexFromContainer(value);
        }

        IEnumerable<IToolbarItem> IToolbarElement.SubItems
            => ItemContainerGenerator.Containers.Select(x => x.ContainerControl).OfType<IToolbarItem>();

        protected ToolbarBase()
        { }

        protected ToolbarBase(IMenuInteractionHandler interactionHandler)
            : base(interactionHandler)
        { }
    }
}
