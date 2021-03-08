//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.Server.Rcon
{
    public readonly struct Packet
    {
        public readonly int Id;

        public readonly PacketType Type;

        public readonly string Body;

        public Packet(int id, PacketType type, string body)
        {
            if (id < 1) throw new ArgumentOutOfRangeException(nameof(id), "ID has to be positive");
            (Id, Type, Body) = (id, type, body);
        }

        internal static int GetBodyLength(int size) => size - 10;

        internal int GetSize() => 10 + (Body?.Length ?? 0);
    }
}
