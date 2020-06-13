//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.Server.Rcon
{
    internal static class StreamExtensions
    {
        private static readonly byte[] Padding = new byte[2];

        private static byte[] GetInt32BytesLE(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes;
        }

        private static int GetInt32FromBytesLE(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private static Task WriteInt32LEAsync(this Stream stream, int value)
        {
            var bytes = GetInt32BytesLE(value);
            return stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private static async Task<int> ReadInt32LEAsync(this Stream stream)
        {
            var bytes = new byte[sizeof(int)];
            await stream.ReadAsync(bytes, 0, bytes.Length);
            return GetInt32FromBytesLE(bytes);
        }

        public static async Task WriteAsync(this Stream stream, Packet packet)
        {
            await stream.WriteInt32LEAsync(packet.GetSize());
            await stream.WriteInt32LEAsync(packet.Id);
            await stream.WriteInt32LEAsync((int)packet.Type);

            byte[] body = Encoding.ASCII.GetBytes(packet.Body);
            await stream.WriteAsync(body, 0, body.Length);

            await stream.WriteAsync(Padding, 0, Padding.Length);
        }

        public static async Task<Packet> ReadPacketAsync(this Stream stream)
        {
            var size = await stream.ReadInt32LEAsync();
            var id = await stream.ReadInt32LEAsync();
            var type = (PacketType)await stream.ReadInt32LEAsync();

            var body = new byte[Packet.GetBodyLength(size)];
            await stream.ReadAsync(body, 0, body.Length);

            await stream.ReadAsync(Padding, 0, Padding.Length);

            try
            {
                return new Packet(id, type, Encoding.ASCII.GetString(body));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new PacketIdException(id, ex);
            }
        }
    }
}
