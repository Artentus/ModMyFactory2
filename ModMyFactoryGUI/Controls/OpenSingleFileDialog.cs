//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Controls
{
    internal class OpenSingleFileDialog : FileSystemDialog
    {
        private sealed class DefaultImpl : ISingleFileSystemDialogImpl
        {
            // This functionality only exists in Windows, so on Unix we fall back to a normal file dialog

            public async Task<string> ShowDialogAsync(OpenSingleFileDialog dialog, Window parent)
            {
                var d = new OpenFileDialog
                {
                    AllowMultiple = false,
                    Directory = dialog.Directory,
                    Title = dialog.Title
                };

                var result = await d.ShowAsync(parent);
                if (result is null) return null;
                if (result.Length < 1) return null;

                string path = result[0];
                return string.Equals(Path.GetFileName(path), dialog.FileName,
                    StringComparison.InvariantCultureIgnoreCase)
                    ? path : null;
            }
        }

        private static readonly ISingleFileSystemDialogImpl dialogImpl;

        public string FileName { get; set; }

        public string FilterName { get; set; }

        static OpenSingleFileDialog()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dialogImpl = new Win32.SingleFileDialogImpl();
            }
            else
            {
                dialogImpl = new DefaultImpl();
            }
        }

        public Task<string> ShowAsync(Window parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            return dialogImpl.ShowDialogAsync(this, parent);
        }
    }
}
