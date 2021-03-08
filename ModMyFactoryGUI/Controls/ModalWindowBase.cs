//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using ModMyFactory.Win32;
using ModMyFactoryGUI.Helpers;
using System.Runtime.InteropServices;

namespace ModMyFactoryGUI.Controls
{
    internal abstract class ModalWindowBase : WindowBase
    {
        protected ModalWindowBase()
        {
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // We need to disable resize buttons manually on Windows since Avalonia doesn't
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var handle = PlatformImpl.Handle.Handle;
                var styles = User32.GetWindowStyles(handle);
                styles = styles.UnsetFlag(WindowStyles.MaximizeBox);
                User32.SetWindowStyles(handle, styles);
            }
        }
    }
}
