//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactoryGUI.Controls
{
    [Flags]
    internal enum DialogOptions
    {
        None = 0x0,
        Ok = 0x1,
        Cancel = 0x2,
        Yes = 0x4,
        No = 0x8,
        Retry = 0x10,
        Abort = 0x20,

        OkCancel = Ok | Cancel,
        YesNo = Yes | No,
        RetryAbort = Retry | Abort,
    }
}
