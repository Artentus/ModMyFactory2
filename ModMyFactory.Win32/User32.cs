//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    public static class User32
    {
        [DllImport("user32.dll", EntryPoint = "PostMessageA", SetLastError = true)]
        private static extern bool PostMessageNative(IntPtr hWnd, uint msg, int wParam, int lParam);

        /// <summary>
        /// Tries to post a message to a window.
        /// </summary>
        public static bool TryPostMessage(IntPtr windowHandle, uint message, int wParam, int lParam)
            => PostMessageNative(windowHandle, message, wParam, lParam);

        /// <summary>
        /// Posts a message to a window.
        /// </summary>
        public static void PostMessage(IntPtr windowHandle, uint message, int wParam, int lParam)
        {
            if (!PostMessageNative(windowHandle, message, wParam, lParam))
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Gets attributes of a window.
        /// </summary>
        public static IntPtr GetWindowLong(IntPtr windowHandle, WindowLongIndex index)
        {
            IntPtr result;
            if (Environment.Is64BitProcess)
                result = GetWindowLong64(windowHandle, (int)index);
            else
                result = GetWindowLong32(windowHandle, (int)index);

            if (result == IntPtr.Zero)
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
            return result;
        }

        /// <summary>
        /// Gets the styles of a window.
        /// </summary>
        public static WindowStyles GetWindowStyles(IntPtr windowHandle)
        {
            var result = GetWindowLong(windowHandle, WindowLongIndex.Style);
            return (WindowStyles)((uint)result);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// Changes attributes of a window.
        /// </summary>
        public static IntPtr SetWindowLong(IntPtr windowHandle, WindowLongIndex index, IntPtr newLong)
        {
            IntPtr result;
            if (Environment.Is64BitProcess)
                result = SetWindowLong64(windowHandle, (int)index, newLong);
            else
                result = SetWindowLong32(windowHandle, (int)index, newLong);

            if (result == IntPtr.Zero)
            {
                int hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
            return result;
        }

        /// <summary>
        /// Sets the styles of a window.
        /// </summary>
        public static void SetWindowStyles(IntPtr windowHandle, WindowStyles styles)
        {
            SetWindowLong(windowHandle, WindowLongIndex.Style, new IntPtr((int)((uint)styles)));
        }
    }
}
