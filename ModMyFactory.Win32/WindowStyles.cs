//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.Win32
{
    [Flags]
    public enum WindowStyles : uint
    {
        Overlapped = 0x00000000,
        Tabstop = 0x00010000,
        Group = 0x00020000,
        ThickFrame = 0x00040000,
        SystemMenu = 0x00080000,
        HorizontalScroll = 0x00100000,
        VerticalScroll = 0x00200000,
        DialogFrame = 0x00400000,
        Border = 0x00800000,
        Maximized = 0x01000000,
        Clipchildren = 0x02000000,
        Clipsiblings = 0x04000000,
        Disabled = 0x08000000,
        Visible = 0x10000000,
        Minimized = 0x20000000,
        Child = 0x40000000,
        Popup = 0x80000000,

        MaximizeBox = 0x00010000,
        MinimizeBox = 0x00020000,

        Tiled = Overlapped,
        SizeBox = ThickFrame,
        Iconic = Minimized,

        Caption = Border | DialogFrame,
        OverlappedWindow = Overlapped | Caption | SystemMenu | ThickFrame | MinimizeBox | MaximizeBox,
        TiledWindow = OverlappedWindow,
        PopupWindow = Popup | Border | SystemMenu,
        ChildWindow = Child
    }
}
