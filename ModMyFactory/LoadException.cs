//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory
{
    /// <summary>
    /// Exception that is thrown when a load operation failed
    /// </summary>
    public abstract class LoadException : ManagerException
    {
        protected LoadException(string message)
            : base(message)
        { }
    }

    /// <summary>
    /// Exception that is thrown when a load operation failed due to the path not existing
    /// </summary>
    public class PathNotFoundException : LoadException
    {
        public PathNotFoundException(string message)
            : base(message)
        { }
    }

    /// <summary>
    /// Exception that is thrown when a load operation failed due to the file being loaded not being a valid mod
    /// </summary>
    public class InvalidModDataException : LoadException
    {
        public InvalidModDataException(string message)
            : base(message)
        { }
    }

    /// <summary>
    /// Exception that is thrown when a load operation failed due to the directory being loaded not containing a valid Factorio instance
    /// </summary>
    public class InvalidFactorioDataException : LoadException
    {
        public InvalidFactorioDataException(string message)
            : base(message)
        { }
    }

    /// <summary>
    /// Exception that is thrown when the Factorio Steam instance is not found
    /// </summary>
    public class SteamInstanceNotFoundException : LoadException
    {
        public SteamInstanceNotFoundException(string message)
            : base(message)
        { }
    }
}
