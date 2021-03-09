//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.Win32
{
    [Flags]
    public enum ChangeNotifyFlags : int
    {
        DWord = 0x0003,
        IdList = 0x0000,
        PathA = 0x0001,
        PathW = 0x0005,
        PrinterA = 0x0002,
        PrinterW = 0x0006,
        Flush = 0x1000,
        FlushNoWait = 0x2000
    }
}
