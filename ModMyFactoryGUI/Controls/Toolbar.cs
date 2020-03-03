//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace ModMyFactoryGUI.Controls
{
    class Toolbar : ToolbarBase
    {
        static readonly ITemplate<IPanel> DefaultPanel =
            new FuncTemplate<IPanel>(() => new StackPanel { Orientation = Orientation.Horizontal });

        static Toolbar()
        {
            ItemsPanelProperty.OverrideDefaultValue<Toolbar>(DefaultPanel);
        }


        public Toolbar()
        { }

        public Toolbar(IMenuInteractionHandler interactionHandler)
            : base(interactionHandler)
        { }

        public override void Close()
        {
            if (!IsOpen) return;

            foreach (var subItem in ((IToolbar)this).SubItems)
                subItem.Close();

            IsOpen = false;
            SelectedIndex = -1;

            RaiseEvent(new RoutedEventArgs
            {
                RoutedEvent = MenuClosedEvent,
                Source = this,
            });
        }

        public override void Open()
        {
            if (IsOpen) return;

            IsOpen = true;

            RaiseEvent(new RoutedEventArgs
            {
                RoutedEvent = MenuOpenedEvent,
                Source = this,
            });
        }

        protected override IItemContainerGenerator CreateItemContainerGenerator()
            => new ToolbarItemContainerGenerator(this);
    }
}
