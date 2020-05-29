//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory
{
    /// <summary>
    /// An exception that is thrown if a feature is not available on the current platform
    /// </summary>
    public class PlatformException : ManagerException
    {
        public PlatformException()
            : base("Feature not available on current platform")
        { }
    }
}
