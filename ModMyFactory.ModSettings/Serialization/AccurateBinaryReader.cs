//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.IO;
using System.Text;

namespace ModMyFactory.ModSettings.Serialization
{
    internal class AccurateBinaryReader : BinaryReader
    {
        public AccurateBinaryReader(Stream stream)
            : base(stream, Encoding.UTF8)
        { }

        private byte[] ReadRaw(int size)
        {
            var buffer = new byte[size];
            if (Read(buffer, 0, size) < size) throw new EndOfStreamException();
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer); // All x86 systems are little endian but others may not be
            return buffer;
        }

        public override bool ReadBoolean() => BitConverter.ToBoolean(ReadRaw(sizeof(bool)), 0);

        public override short ReadInt16() => BitConverter.ToInt16(ReadRaw(sizeof(short)), 0);

        public override ushort ReadUInt16() => BitConverter.ToUInt16(ReadRaw(sizeof(ushort)), 0);

        public override int ReadInt32() => BitConverter.ToInt32(ReadRaw(sizeof(int)), 0);

        public override uint ReadUInt32() => BitConverter.ToUInt32(ReadRaw(sizeof(uint)), 0);

        public override long ReadInt64() => BitConverter.ToInt64(ReadRaw(sizeof(long)), 0);

        public override ulong ReadUInt64() => BitConverter.ToUInt64(ReadRaw(sizeof(ulong)), 0);

        public override float ReadSingle() => BitConverter.ToSingle(ReadRaw(sizeof(float)), 0);

        public override double ReadDouble() => BitConverter.ToDouble(ReadRaw(sizeof(double)), 0);

        public override string ReadString()
        {
            if (ReadBoolean()) return string.Empty; // Factorio optimization for empty strings.

            uint length = ReadByte();
            if (length == byte.MaxValue) length = ReadUInt32(); // Factorio optimization for short string lengths.

            var buffer = new byte[length];
            if (Read(buffer, 0, (int)length) < length) throw new EndOfStreamException();
            return Encoding.UTF8.GetString(buffer);
        }


        // Unsupported data types
        public override char ReadChar() => throw new NotSupportedException();

        public override char[] ReadChars(int count) => throw new NotSupportedException();

        public override int PeekChar() => throw new NotSupportedException();

        public override int Read(char[] buffer, int index, int count) => throw new NotSupportedException();

        public override int Read() => throw new NotSupportedException();

        public override decimal ReadDecimal() => throw new NotSupportedException();
    }
}
