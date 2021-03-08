//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.Server.Rcon
{
    /// <summary>
    /// Thrown if an RCON protocol error occurrs
    /// </summary>
    public class RconException : Exception
    {
        public RconException(string message, Exception? innerException = null)
            : base(message, innerException)
        { }
    }

    public class PacketIdException : RconException
    {
        /// <summary>
        /// The ID that caused the exception
        /// </summary>
        public int Id { get; }

        public PacketIdException(int id, Exception? innerException = null)
            : base("Invalid packet ID", innerException)
            => Id = id;
    }

    public class PacketTypeException : RconException
    {
        /// <summary>
        /// The packet that caused the exception
        /// </summary>
        public Packet Packet { get; }

        public PacketTypeException(Packet packet, Exception? innerException = null)
            : base("Invalid packet type", innerException)
            => Packet = packet;
    }
}
