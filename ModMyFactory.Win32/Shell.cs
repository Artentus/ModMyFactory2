//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ModMyFactory.Win32
{
    public static class Shell
    {
        // We have to invoke manually using reflection because the dynamic type does not support COM objects in .Net Core
        private static object Call(this object obj, string methodName, params object[] parameters)
        {
            var t = obj.GetType();
            return t.InvokeMember(methodName, BindingFlags.InvokeMethod, null, obj, parameters);
        }

        private static void Set(this object obj, string propertyName, object value)
        {
            var t = obj.GetType();
            t.InvokeMember(propertyName, BindingFlags.SetProperty, null, obj, new[] { value });
        }

        public static void CreateSymbolicLink(string linkPath, string targetPath, string arguments, string iconLocation)
        {
            // Create the Windows script host shell object
            Type shellType = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
            object shell = Activator.CreateInstance(shellType);

            try
            {
                object shortcut = shell.Call("CreateShortcut", linkPath);

                try
                {
                    shortcut.Set("TargetPath", targetPath);
                    shortcut.Set("Arguments", arguments);
                    shortcut.Set("IconLocation", iconLocation);
                    shortcut.Call("Save");
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
