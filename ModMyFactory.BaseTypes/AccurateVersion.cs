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
    public struct AccurateVersion : IEquatable<AccurateVersion>, IComparable, IComparable<AccurateVersion>
    {
        [FieldOffset(0)]
        public ulong Binary;

        [FieldOffset(0)]
        public ushort Revision;
        [FieldOffset(2)]
        public ushort Minor;
        [FieldOffset(4)]
        public ushort Major;
        [FieldOffset(6)]
        public ushort Main;

        public AccurateVersion(ulong binary)
        {
            Main = 0;
            Major = 0;
            Minor = 0;
            Revision = 0;
            Binary = binary;
        }

        public AccurateVersion(ushort main, ushort major, ushort minor = 0, ushort revision = 0)
        {
            Binary = 0;
            Main = main;
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public bool Equals(AccurateVersion other) => other.Binary == this.Binary;

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is AccurateVersion) return Equals((AccurateVersion)obj);
            else return false;
        }

        public int CompareTo(AccurateVersion other) => this.Binary.CompareTo(other.Binary);

        public int CompareTo(object obj)
        {
            if (obj is null) return int.MaxValue;
            if (obj is AccurateVersion) return CompareTo((AccurateVersion)obj);
            else throw new ArgumentException();
        }

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
    }
}
