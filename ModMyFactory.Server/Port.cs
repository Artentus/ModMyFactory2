//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.Server
{
    /// <summary>
    /// A network port
    /// </summary>
    public readonly struct Port : IEquatable<Port>
    {
        private readonly bool _hasValue;

        /// <summary>
        /// A value representing an unspecified port
        /// </summary>
        public static readonly Port None = new Port();

        /// <summary>
        /// The port number
        /// </summary>
        public readonly ushort Number;

        /// <summary>
        /// Whether this port is not specified
        /// </summary>
        public bool IsNone => !_hasValue;

        public Port(ushort number)
            => (_hasValue, Number) = (true, number);

        public static bool operator ==(Port first, Port second)
            => first.Equals(second);

        public static bool operator !=(Port first, Port second)
            => !first.Equals(second);

        public static implicit operator Port(ushort value)
            => new Port(value);

        public static implicit operator Port(int value)
        {
            if ((value < 0) || (value > ushort.MaxValue))
                throw new ArgumentOutOfRangeException(nameof(value));
            return new Port((ushort)value);
        }

        public bool Equals(Port other)
        {
            if (_hasValue != other._hasValue) return false;
            return Number == other.Number;
        }

        public override bool Equals(object obj)
        {
            if (obj is Port other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            if (IsNone) return int.MinValue;
            return Number;
        }

        public override string ToString()
        {
            if (IsNone) return nameof(None);
            return Number.ToString();
        }
    }
}
