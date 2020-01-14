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
    }
}
