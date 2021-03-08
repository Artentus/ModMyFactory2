//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.Server.Rcon
{
    public enum PacketType : int
    {
        ResponseValue = 0,
        ExecuteCommand = 2,
        AuthResponse = 2,
        Auth = 3
    }
}
