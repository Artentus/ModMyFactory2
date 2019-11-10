namespace ModMyFactory
{
    /// <summary>
    /// An exception that is thrown if a feature is not available on the current platform.
    /// </summary>
    public class PlatformException : ManagerException
    {
        public PlatformException()
            : base("Feature not available on current platform.")
        { }
    }
}
