//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Win32.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Controls.Win32
{
    internal class SingleFileDialogImpl : ISingleFileSystemDialogImpl
    {
        private const UnmanagedMethods.FOS DefaultDialogOptions
            = UnmanagedMethods.FOS.FOS_FORCEFILESYSTEM
            | UnmanagedMethods.FOS.FOS_NOVALIDATE
            | UnmanagedMethods.FOS.FOS_NOTESTFILECREATE
            | UnmanagedMethods.FOS.FOS_DONTADDTORECENT;

        private static string? GetAbsoluteFilePath(UnmanagedMethods.IShellItem shellItem)
        {
            if (shellItem.GetDisplayName(UnmanagedMethods.SIGDN_FILESYSPATH, out var pszString) == (uint)UnmanagedMethods.HRESULT.S_OK)
            {
                if (pszString != IntPtr.Zero)
                {
                    try
                    {
                        return Marshal.PtrToStringAuto(pszString);
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pszString);
                    }
                }
            }

            return null;
        }

        public unsafe Task<string?> ShowDialogAsync(OpenSingleFileDialog dialog, Window parent)
        {
            var hWnd = parent?.PlatformImpl?.Handle?.Handle ?? IntPtr.Zero;
            return Task.Factory.StartNew(() =>
            {
                Guid clsid = UnmanagedMethods.ShellIds.OpenFileDialog;
                Guid iid = UnmanagedMethods.ShellIds.IFileDialog;
                UnmanagedMethods.CoCreateInstance(ref clsid, IntPtr.Zero, 1, ref iid, out var unk);
                var frm = (UnmanagedMethods.IFileDialog)unk;

                frm.GetOptions(out uint options);
                options |= (uint)DefaultDialogOptions;
                frm.SetOptions(options);
                frm.SetTitle(dialog.Title ?? string.Empty);

                var filters = new[]
                {
                    new UnmanagedMethods.COMDLG_FILTERSPEC { pszName = dialog.FilterName ?? string.Empty, pszSpec = dialog.FileName ?? "*.*" }
                };
                frm.SetFileTypes((uint)filters.Length, filters);
                frm.SetFileTypeIndex(0);

                if (dialog.Directory != null)
                {
                    Guid riid = UnmanagedMethods.ShellIds.IShellItem;
                    if (UnmanagedMethods.SHCreateItemFromParsingName(dialog.Directory, IntPtr.Zero, ref riid, out var directoryShellItem) == (uint)UnmanagedMethods.HRESULT.S_OK)
                    {
                        frm.SetFolder(directoryShellItem);
                        frm.SetDefaultFolder(directoryShellItem);
                    }
                }

                if (frm.Show(hWnd) == (uint)UnmanagedMethods.HRESULT.S_OK)
                {
                    if (frm.GetResult(out var shellItem) == (uint)UnmanagedMethods.HRESULT.S_OK)
                        return GetAbsoluteFilePath(shellItem);
                }

                return null;
            });
        }
    }
}
