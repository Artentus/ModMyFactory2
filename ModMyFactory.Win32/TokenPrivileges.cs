﻿using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TokenPrivileges
    {
        public int PrivilegeCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public Privilege[] Privileges;
    }
}
