﻿using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Privilege
    {
        public const int EnabledAttribute = 2;

        public Luid Luid;
        public int Attributes;
    }
}
