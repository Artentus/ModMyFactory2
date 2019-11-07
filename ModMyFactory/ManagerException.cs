using System;

namespace ModMyFactory
{
    /// <summary>
    /// An exception thrown by the ModMyFactory core system.
    /// </summary>
    public class ManagerException : Exception
    {
        public ManagerException(string message)
            : base(message)
        { }
    }
}
