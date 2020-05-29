//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory
{
    /// <summary>
    /// Exception thrown by the ModMyFactory core system
    /// </summary>
    public abstract class ManagerException : Exception
    {
        public ManagerException(string message)
            : base(message)
        { }
    }
}
