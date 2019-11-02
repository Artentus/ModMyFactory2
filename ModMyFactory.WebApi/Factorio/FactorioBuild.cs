namespace ModMyFactory.WebApi.Factorio
{
    /// <summary>
    /// Specifies the build of Factorio.
    /// </summary>
    public enum FactorioBuild
    {
        /// <summary>
        /// The main release build of Factorio.
        /// </summary>
        Alpha,

        /// <summary>
        /// The demo build of Factorio.
        /// </summary>
        Demo,

        /// <summary>
        /// A build of Factorio with no graphical user interface intended for use on servers.
        /// </summary>
        Headless
    }
}
