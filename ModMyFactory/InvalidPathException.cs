namespace ModMyFactory
{
    /// <summary>
    /// Exception that is thrown if it was attempted to load a directory or file that did not contain valid data.
    /// </summary>
    public class InvalidPathException : ManagerException
    {
        public InvalidPathException(string message)
            : base(message)
        { }
    }
}
