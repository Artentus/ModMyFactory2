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
    public static class Shell
    {
        public static void CreateSymbolicLink(string linkPath, string targetPath, string arguments, string iconLocation)
        {
            // Create the windows script host shell object
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
            dynamic shell = Activator.CreateInstance(t);

            try
            {
                dynamic shortcut = shell.CreateShortcut(linkPath);

                try
                {
                    shortcut.TargetPath = targetPath;
                    shortcut.Arguments = arguments;
                    shortcut.IconLocation = iconLocation;
                    shortcut.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(shortcut);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
