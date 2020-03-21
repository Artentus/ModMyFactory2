using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace ModMyFactoryGUI
{
    internal readonly struct SHA1Hash : IEquatable<SHA1Hash>
    {
        private const int SHA1Size = 20; // Size of a SHA1 hash in bytes

        public readonly ReadOnlyMemory<byte> Bytes;

        public SHA1Hash(byte[] bytes)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length != SHA1Size) throw new ArgumentException("Incorrect number of bytes", nameof(bytes));

            Bytes = bytes.AsMemory();
        }

        public bool Equals(SHA1Hash other)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj is SHA1Hash other) return Equals(other);
            else return false;
        }

        public override int GetHashCode()
        {
            // This object is itself a hash, so we just take the first 4 bytes
            if (MemoryMarshal.TryGetArray(Bytes, out var segment))
            {
                return BitConverter.ToInt32(segment.Array, 0);
            }
            else
            {
                return 0;
            }
        }

        public override string ToString()
        {
            if (MemoryMarshal.TryGetArray(Bytes, out var segment))
            {
                // Each byte produces 2 characters
                var sb = new StringBuilder(SHA1Size * 2);
                foreach (byte b in segment)
                    sb.AppendFormat("{0:x2}", b);

                return sb.ToString();
            }

            return null;
        }

        public static bool TryParse(string s, out SHA1Hash result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (s.Length != SHA1Size * 2) return false;

            byte[] bytes = new byte[SHA1Size];
            for (int i = 0; i < SHA1Size; i++)
            {
                string byteString = s.Substring(i * 2, 2);
                if (byte.TryParse(byteString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b)) bytes[i] = b;
                else return false;
            }

            result = new SHA1Hash(bytes);
            return true;
        }

        public static SHA1Hash Parse(string s)
        {
            if (TryParse(s, out var result)) return result;
            else throw new FormatException();
        }

        public static bool operator ==(SHA1Hash first, SHA1Hash second)
            => first.Equals(second);

        public static bool operator !=(SHA1Hash first, SHA1Hash second)
            => first.Equals(second);
    }
}
