//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Win32;
using System;

namespace ModMyFactoryGUI.Helpers
{
    // Handling of the standard output stream on Windows is horrible so we need all of this code to somewhat make it work.
    internal static class ConsoleHelper
    {
        public static bool TryAttachConsole(out IntPtr consoleHandle)
        {
            consoleHandle = IntPtr.Zero;
#if NETFULL
            bool result = Kernel32.TryAttachConsole();
            if (result)
            {
                consoleHandle = Kernel32.GetConsoleWindow();
                Console.WriteLine();
                Console.WriteLine();
            }
            return result;
#elif NETCORE
            bool result = false;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                result = Kernel32.TryAttachConsole();
                if (result)
                {
                    consoleHandle = Kernel32.GetConsoleWindow();
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
            return result;
#endif
        }

        public static void FreeConsole(IntPtr consoleHandle)
        {
            const uint WM_CHAR = 0x0102;
            const int VK_ENTER = 0x0D;
#if NETFULL
            User32.PostMessage(consoleHandle, WM_CHAR, VK_ENTER, 0);
            Kernel32.FreeConsole();
#elif NETCORE
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                User32.PostMessage(consoleHandle, WM_CHAR, VK_ENTER, 0);
                Kernel32.FreeConsole();
            }
#endif
        }
    }
}
