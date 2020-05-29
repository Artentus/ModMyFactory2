//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory
{
    /// <summary>
    /// Exception that is thrown if it was attempted to load a directory or file that did not contain valid data
    /// </summary>
    public class InvalidPathException : ManagerException
    {
        public InvalidPathException(string message)
            : base(message)
        { }
    }
}
