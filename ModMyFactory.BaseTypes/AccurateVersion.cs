//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// A version representation identical in behavior to the one Factorio uses.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [JsonConverter(typeof(AccurateVersionConverter))]
    public readonly struct AccurateVersion : IEquatable<AccurateVersion>, IComparable, IComparable<AccurateVersion>
    {
        [FieldOffset(0)]
        public readonly ulong Binary;

        [FieldOffset(0)]
        public readonly ushort Revision;

        [FieldOffset(2)]
        public readonly ushort Minor;

        [FieldOffset(4)]
        public readonly ushort Major;

        [FieldOffset(6)]
        public readonly ushort Main;

        public AccurateVersion(ulong binary)
        {
            (Main, Major, Minor, Revision) = (0, 0, 0, 0);
            Binary = binary;
        }

        public AccurateVersion(ushort main, ushort major, ushort minor = 0, ushort revision = 0)
        {
            Binary = 0;
            (Main, Major, Minor, Revision) = (main, major, minor, revision);
        }

        /// <summary>
        /// Creates a version object containing only the main and major version parts of this version object.
        /// </summary>
        public AccurateVersion ToMajor() => new AccurateVersion(Binary & 0xFFFFFFFF00000000);

        public void Deconstruct(out ushort main, out ushort major, out ushort minor, out ushort revision)
            => (main, major, minor, revision) = (Main, Major, Minor, Revision);

        public bool Equals(AccurateVersion other) => other.Binary == this.Binary;

        public override bool Equals(object obj)
            => (obj is AccurateVersion other)
            ? Equals(other)
            : false;

        public int CompareTo(AccurateVersion other) => this.Binary.CompareTo(other.Binary);

        public int CompareTo(object obj)
            => (obj is AccurateVersion other)
            ? CompareTo(other)
            : throw new ArgumentException("Value needs to be of type AccurateVersion", nameof(obj));

        public override int GetHashCode() => Binary.GetHashCode();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Main);
            sb.Append('.');
            sb.Append(Major);

            if ((Minor > 0) || (Revision > 0))
            {
                sb.Append('.');
                sb.Append(Minor);

                if (Revision > 0)
                {
                    sb.Append('.');
                    sb.Append(Revision);
                }
            }

            return sb.ToString();
        }

        public string ToString(int fieldCount)
        {
            if ((fieldCount < 1) || (fieldCount > 4)) throw new ArgumentOutOfRangeException(nameof(fieldCount));

            var sb = new StringBuilder();
            sb.Append(Main);

            if (fieldCount > 1)
            {
                sb.Append('.');
                sb.Append(Major);

                if (fieldCount > 2)
                {
                    sb.Append('.');
                    sb.Append(Minor);

                    if (fieldCount > 3)
                    {
                        sb.Append('.');
                        sb.Append(Revision);
                    }
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Tries to interpret a string as a version.
        /// </summary>
        public static bool TryParse(string value, out AccurateVersion version)
        {
            version = default;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var parts = value.Split('.');
            if (parts.Length > 4) return false;

            if (!ushort.TryParse(parts[0], out var main)) return false;
            ushort major = 0;
            if ((parts.Length > 1) && !ushort.TryParse(parts[1], out major)) return false;
            ushort minor = 0;
            if ((parts.Length > 2) && !ushort.TryParse(parts[2], out minor)) return false;
            ushort revision = 0;
            if ((parts.Length > 3) && !ushort.TryParse(parts[3], out revision)) return false;

            version = new AccurateVersion(main, major, minor, revision);
            return true;
        }

        /// <summary>
        /// Interprets a string as a version.
        /// </summary>
        public static AccurateVersion Parse(string value)
        {
            if (TryParse(value, out var result)) return result;
            else throw new FormatException();
        }


        public static bool operator ==(AccurateVersion first, AccurateVersion second) => first.Equals(second);

        public static bool operator !=(AccurateVersion first, AccurateVersion second) => !first.Equals(second);

        public static bool operator <(AccurateVersion first, AccurateVersion second) => first.CompareTo(second) < 0;

        public static bool operator >(AccurateVersion first, AccurateVersion second) => first.CompareTo(second) > 0;

        public static bool operator <=(AccurateVersion first, AccurateVersion second) => first.CompareTo(second) <= 0;

        public static bool operator >=(AccurateVersion first, AccurateVersion second) => first.CompareTo(second) >= 0;

        public static implicit operator AccurateVersion(ValueTuple<ushort, ushort, ushort, ushort> parts)
            => new AccurateVersion(parts.Item1, parts.Item2, parts.Item3, parts.Item4);

        public static implicit operator AccurateVersion(ValueTuple<ushort, ushort> parts)
            => new AccurateVersion(parts.Item1, parts.Item2);

        public static implicit operator AccurateVersion(ValueTuple<int, int, int, int> parts)
            => new AccurateVersion((ushort)parts.Item1, (ushort)parts.Item2, (ushort)parts.Item3, (ushort)parts.Item4);

        public static implicit operator AccurateVersion(ValueTuple<int, int> parts)
            => new AccurateVersion((ushort)parts.Item1, (ushort)parts.Item2);
    }
}
