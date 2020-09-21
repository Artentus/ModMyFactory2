//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactoryGUI.Update
{
    sealed class TagBranch : IEquatable<TagBranch>
    {
        private readonly string _name;

        /// <summary>
        /// Alpha release, not feature complete
        /// </summary>
        public static readonly TagBranch Alpha = new TagBranch("alpha");

        /// <summary>
        /// Beta release, feature complete but may be unstable
        /// </summary>
        public static readonly TagBranch Beta = new TagBranch("beta");

        /// <summary>
        /// Stable release
        /// </summary>
        public static readonly TagBranch Stable = new TagBranch(string.Empty);

        /// <summary>
        /// Upcomming release, not yet stable
        /// </summary>
        public static readonly TagBranch Prerelease = new TagBranch("pre");

        private TagBranch(in string name)
            => _name = name;

        public override string ToString() => _name;

        public static bool TryParse(in string value, out TagBranch result)
        {
            if (string.Equals(value, Alpha._name, StringComparison.OrdinalIgnoreCase))
            {
                result = Alpha;
                return true;
            }
            else if (string.Equals(value, Beta._name, StringComparison.OrdinalIgnoreCase))
            {
                result = Beta;
                return true;
            }
            else if (string.IsNullOrEmpty(value))
            {
                result = Stable;
                return true;
            }
            else if (string.Equals(value, Prerelease._name, StringComparison.OrdinalIgnoreCase))
            {
                result = Prerelease;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public static TagBranch Parse(in string value)
        {
            if (TryParse(value, out var result)) return result;
            else throw new FormatException();
        }

        public bool Equals(TagBranch other)
        {
            if (other is null) return false;
            return string.Equals(this._name, other._name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj is TagBranch other) return Equals(other);
            else return false;
        }

        public override int GetHashCode() => _name.GetHashCode();

        public static bool operator ==(TagBranch first, TagBranch second)
        {
            if (first is null) return second is null;
            else return first.Equals(second);
        }

        public static bool operator !=(TagBranch first, TagBranch second)
        {
            if (first is null) return !(second is null);
            else return !first.Equals(second);
        }
    }
}
