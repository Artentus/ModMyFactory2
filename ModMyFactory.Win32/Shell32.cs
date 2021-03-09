//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    public static class Shell32
    {
        [DllImport("shell32.dll", EntryPoint = "SHChangeNotify")]
        public static extern void ChangeNotify(ChangeNotifyEventId eventId, ChangeNotifyFlags flags, IntPtr item1, IntPtr item2);

        public static void ChangeNotify(ChangeNotifyEventId eventId, ChangeNotifyFlags flags)
            => ChangeNotify(eventId, flags, IntPtr.Zero, IntPtr.Zero);
    }
}
