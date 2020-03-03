//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.Win32
{
    public enum WindowLongIndex : int
    {
        WindowProcedure = -4,
        InstanceHandle = -6,
        Id = -12,
        Style = -16,
        ExtendedStyle = -20,
        UserData = -21,

        // Only for message boxes
        MessageResult = 0x0,
        DialogProcedure = 0x4,
        User = 0x8
    }
}
