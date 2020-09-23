//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Text;

namespace ModMyFactoryGUI.Update
{
    sealed class TagVersion : IEquatable<TagVersion>, IComparable<TagVersion>, IComparable
    {
        public readonly uint Major;
        public readonly uint Minor;
        public readonly uint Revision;
        public readonly uint Build;
        public readonly TagBranch Branch;

        public bool IsPrerelease => Branch == TagBranch.Prerelease;

        public TagVersion(uint major, uint minor, uint revision, uint build, TagBranch branch)
            => (Major, Minor, Revision, Build, Branch) = (major, minor, revision, build, branch);

        public TagVersion(uint major, uint minor, uint revision, uint build)
            : this(major, minor, revision, build, TagBranch.Stable)
        { }

        public TagVersion WithMajor(uint major) => new TagVersion(major, this.Minor, this.Revision, this.Build, this.Branch);

        public TagVersion WithMinor(uint minor) => new TagVersion(this.Major, minor, this.Revision, this.Build, this.Branch);

        public TagVersion WithRevision(uint revision) => new TagVersion(this.Major, this.Minor, revision, this.Build, this.Branch);

        public TagVersion WithBuild(uint build) => new TagVersion(this.Major, this.Minor, this.Revision, build, this.Branch);

        public TagVersion WithBranch(TagBranch branch) => new TagVersion(this.Major, this.Minor, this.Revision, this.Build, branch);

        public void Deconstruct(out uint major, out uint minor, out uint revision, out uint build, out TagBranch branch)
            => (major, minor, revision, build, branch) = (Major, Minor, Revision, Build, Branch);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Major);
            sb.Append('.');
            sb.Append(Minor);
            sb.Append('.');
            sb.Append(Revision);
            sb.Append('.');
            sb.Append(Build);

            string branchStr = Branch.ToString();
            if (!string.IsNullOrEmpty(branchStr))
            {
                sb.Append('-');
                sb.Append(branchStr);
            }

            return sb.ToString();
        }

        public static bool TryParse(in string value, out TagVersion result)
        {
            result = null;

            var branch = TagBranch.Stable;
            int index = value.LastIndexOf('-');
            if (index >= 0)
            {
                int branchIndex = index + 1;
                if (branchIndex == value.Length) return false;

                string branchStr = value.Substring(branchIndex);
                if (!TagBranch.TryParse(branchStr, out branch)) return false;
            }
            else
            {
                index = value.Length;
            }

            if (index <= 0) return false;
            string versionString = value.Substring(0, index);
            var parts = versionString.Split('.');
            if (parts.Length != 4) return false;
            if (!uint.TryParse(parts[0], out uint major)) return false;
            if (!uint.TryParse(parts[1], out uint minor)) return false;
            if (!uint.TryParse(parts[2], out uint revision)) return false;
            if (!uint.TryParse(parts[3], out uint build)) return false;

            result = new TagVersion(major, minor, revision, build, branch);
            return true;
        }

        public static TagVersion Parse(in string value)
        {
            if (TryParse(value, out var result)) return result;
            else throw new FormatException();
        }

        public bool Equals(TagVersion other)
        {
            if (other is null) return false;
            return (this.Major == other.Major)
                && (this.Minor == other.Minor)
                && (this.Revision == other.Revision)
                && (this.Build == other.Build)
                && (this.Branch == other.Branch);
        }

        public override bool Equals(object obj)
        {
            if (obj is TagVersion other) return Equals(other);
            else return false;
        }

        public override int GetHashCode()
            => Major.GetHashCode()
             ^ Minor.GetHashCode()
             ^ Revision.GetHashCode()
             ^ Build.GetHashCode()
             ^ Branch.GetHashCode();

        public static bool operator ==(TagVersion first, TagVersion second)
        {
            if (first is null) return second is null;
            else return first.Equals(second);
        }

        public static bool operator !=(TagVersion first, TagVersion second)
        {
            if (first is null) return !(second is null);
            else return !first.Equals(second);
        }

        public int CompareTo(TagVersion other)
        {
            if (other is null) return int.MaxValue;
            else return this.Build.CompareTo(other.Build);
        }

        public int CompareTo(object obj)
        {
            if (obj is TagVersion other) return CompareTo(other);
            else throw new ArgumentException("Object must be of type TagVersion", nameof(obj));
        }

        public static bool operator >(TagVersion first, TagVersion second)
        {
            if (first is null) return false;
            else return first.CompareTo(second) > 0;
        }

        public static bool operator <(TagVersion first, TagVersion second)
        {
            if (first is null) return !(second is null);
            else return first.CompareTo(second) < 0;
        }

        public static bool operator >=(TagVersion first, TagVersion second)
        {
            if (first is null) return second is null;
            else return first.CompareTo(second) >= 0;
        }

        public static bool operator <=(TagVersion first, TagVersion second)
        {
            if (first is null) return true;
            else return first.CompareTo(second) <= 0;
        }
    }
}
