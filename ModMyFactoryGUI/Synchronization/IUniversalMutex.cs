//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Runtime.InteropServices;

namespace ModMyFactoryGUI.Synchronization
{
    internal interface IUniversalMutex : IDisposable
    {
        bool TryAquire();

        void Release();
    }

    internal static class UniversalMutex
    {
        public static IUniversalMutex CreateForCurrentPlatform(string uid)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new Win32.Mutex(uid);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new Unix.Mutex();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}