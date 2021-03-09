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
    public enum ChangeNotifyEventId : int
    {
        AllEvents = 0x7FFFFFFF,
        AssociationChanged = 0x08000000,
        Attributes = 0x00000800,
        Create = 0x00000002,
        Delete = 0x00000004,
        DriveAdd = 0x00000100,
        DriveAddGui = 0x00010000,
        DriveRemoved = 0x00000080,
        ExtendedEvent = 0x04000000,
        FreeSpace = 0x00040000,
        MediaInserted = 0x00000020,
        MediaRemoved = 0x00000040,
        MakeDirectory = 0x00000008,
        NetShare = 0x00000200,
        NetUnshare = 0x00000400,
        RenameFolder = 0x00020000,
        RenameItem = 0x00000001,
        RemoveDirectory = 0x00000010,
        ServerDisconnect = 0x00004000,
        UpdateDirectory = 0x00001000,
        UpdateImage = 0x00008000,
    }
}
