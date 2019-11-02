using System;
using System.IO;
using System.Text;

namespace ModMyFactory.ModSettings.Serialization
{
    class AccurateBinaryWriter : BinaryWriter
    {
        public AccurateBinaryWriter(Stream stream)
            : base(stream, Encoding.UTF8)
        { }

        void WriteRaw(byte[] buffer)
        {
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer); // All x86 systems are little endian but others may not be
            Write(buffer);
        }

        public override void Write(bool value) => WriteRaw(BitConverter.GetBytes(value));
        public override void Write(short value) => WriteRaw(BitConverter.GetBytes(value));
        public override void Write(ushort value) => WriteRaw(BitConverter.GetBytes(value));
        public override void Write(int value) => WriteRaw(BitConverter.GetBytes(value));
        public override void Write(uint value) => WriteRaw(BitConverter.GetBytes(value));
        public override void Write(long value) => WriteRaw(BitConverter.GetBytes(value));
        public override void Write(ulong value) => WriteRaw(BitConverter.GetBytes(value));
        public override void Write(float value) => WriteRaw(BitConverter.GetBytes(value));
        public override void Write(double value) => WriteRaw(BitConverter.GetBytes(value));

        public override void Write(string value)
        {
            bool isEmpty = string.IsNullOrEmpty(value);
            Write(isEmpty);
            if (isEmpty) return; // Factorio optimization for empty strings.

            var buffer = Encoding.UTF8.GetBytes(value);
            if (buffer.Length < byte.MaxValue) // Factorio optimization for short string lengths.
            {
                Write((byte)buffer.Length);
            }
            else
            {
                Write(byte.MaxValue);
                Write((uint)buffer.Length);
            }
            Write(buffer);
        }


        // Unsupported data types
        public override void Write(char value) => throw new NotSupportedException();
        public override void Write(char[] chars) => throw new NotSupportedException();
        public override void Write(char[] chars, int index, int count) => throw new NotSupportedException();
        public override void Write(decimal value) => throw new NotSupportedException();
    }
}
