//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Net;

namespace ModMyFactory.Server
{
    /// <summary>
    /// Stores information about a web server binding
    /// </summary>
    public sealed class BindingTarget : IEquatable<BindingTarget>
    {
        /// <summary>
        /// The IP address of the binding
        /// </summary>
        public IPAddress Address { get; }

        /// <summary>
        /// Optional port of the binding
        /// </summary>
        public Port Port { get; }

        public BindingTarget(IPAddress address, Port port)
        {
            if (address is null) throw new ArgumentNullException(nameof(address));
            (Address, Port) = (address, port);
        }

        public BindingTarget(IPAddress address)
            : this(address, Port.None)
        { }

        public bool Equals(BindingTarget other)
        {
            if (other is null) return false;
            return Address.Equals(other.Address) && (Port == other.Port);
        }

        public override bool Equals(object obj)
        {
            if (obj is BindingTarget other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
            => Address.GetHashCode() ^ Port.GetHashCode();

        public override string ToString()
        {
            if (Port.IsNone) return Address.ToString();
            else return $"{Address}:{Port}";
        }
    }
}
