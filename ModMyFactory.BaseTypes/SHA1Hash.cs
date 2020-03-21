//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// A SHA1 hash
    /// </summary>
    public readonly struct SHA1Hash : IEquatable<SHA1Hash>
    {
        private const int SHA1Size = 20; // Size of a SHA1 hash in bytes

        private readonly byte[] _bytes;

        /// <param name="bytes">The 20 hash bytes</param>
        public SHA1Hash(byte[] bytes)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length != SHA1Size) throw new ArgumentException("Incorrect number of bytes", nameof(bytes));

            _bytes = bytes;
        }

        /// <summary>
        /// Tries to parse a string as a SHA1 hash
        /// </summary>
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

        /// <summary>
        /// Parses a string as a SHA1 hash
        /// </summary>
        public static SHA1Hash Parse(string s)
        {
            if (TryParse(s, out var result)) return result;
            else throw new FormatException();
        }

        public static bool operator ==(SHA1Hash first, SHA1Hash second)
            => first.Equals(second);

        public static bool operator !=(SHA1Hash first, SHA1Hash second)
            => first.Equals(second);

        public bool Equals(SHA1Hash other)
        {
            var firstBytes = (IStructuralEquatable)this._bytes;
            var secondBytes = (IStructuralEquatable)other._bytes;
            return firstBytes.Equals(secondBytes, EqualityComparer<byte>.Default);
        }

        public override bool Equals(object obj)
        {
            if (obj is SHA1Hash other) return Equals(other);
            else return false;
        }

        public override int GetHashCode()
        {
            // This object is itself a hash, so we just take the first 4 bytes
            return BitConverter.ToInt32(_bytes, 0);
        }

        public override string ToString()
        {
            // Each byte produces 2 characters
            var sb = new StringBuilder(SHA1Size * 2);
            for (int i = 0; i < SHA1Size; i++)
                sb.AppendFormat("{0:x2}", _bytes[i]);

            return sb.ToString();
        }
    }
}
