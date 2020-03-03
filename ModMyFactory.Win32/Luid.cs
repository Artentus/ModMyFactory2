//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Luid
    {
        public uint Low;
        public int High;
    }
}
