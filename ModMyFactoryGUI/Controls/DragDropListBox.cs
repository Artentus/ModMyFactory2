//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System;

namespace ModMyFactoryGUI.Controls
{
    internal class DragDropListBox : ListBox, IStyleable
    {
        public event EventHandler<PointerPressedEventArgs>? PreviewPointerPressed;

        public event EventHandler<PointerReleasedEventArgs>? PreviewPointerReleased;

        public event EventHandler<PointerEventArgs>? PreviewPointerMoved;

        public event EventHandler<DragEventArgs>? DragOver;

        public event EventHandler<DragEventArgs>? Drop;

        Type IStyleable.StyleKey => typeof(ListBox);

        public DragDropListBox()
        {
            AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);
            AddHandler(PointerReleasedEvent, OnPreviewPointerReleased, RoutingStrategies.Tunnel);
            AddHandler(PointerMovedEvent, OnPreviewPointerMoved, RoutingStrategies.Tunnel);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);
            AddHandler(DragDrop.DropEvent, OnDrop);
        }

        protected virtual void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
            => PreviewPointerPressed?.Invoke(sender, e);

        protected virtual void OnPreviewPointerReleased(object? sender, PointerReleasedEventArgs e)
            => PreviewPointerReleased?.Invoke(sender, e);

        protected virtual void OnPreviewPointerMoved(object? sender, PointerEventArgs e)
            => PreviewPointerMoved?.Invoke(sender, e);

        protected virtual void OnDragOver(object? sender, DragEventArgs e)
            => DragOver?.Invoke(sender, e);

        protected virtual void OnDrop(object? sender, DragEventArgs e)
            => Drop?.Invoke(sender, e);
    }
}
