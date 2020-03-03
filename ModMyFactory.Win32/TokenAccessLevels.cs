//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.Win32
{
    [Flags]
    public enum TokenAccessLevels
    {
        AssignPrimary = 0x0001,
        Duplicate = 0x0002,
        Impersonate = 0x0004,
        Query = 0x0008,
        QuerySource = 0x0010,
        AdjustPrivileges = 0x0020,
        AdjustGroups = 0x0040,
        AdjustDefault = 0x0080,
        AdjustSessionId = 0x0100,

        Read = 0x00020000 | Query,
        Write = 0x00020000 | AdjustPrivileges | AdjustGroups | AdjustDefault,

        AllAccess = 0x000F0000 | AssignPrimary | Duplicate | Impersonate
                               | Query | QuerySource | AdjustPrivileges
                               | AdjustGroups | AdjustDefault | AdjustSessionId,

        MaximumAllowed = 0x02000000
    }
}
