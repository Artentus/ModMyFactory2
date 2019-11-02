namespace ModMyFactory.WebApi.Factorio
{
    /// <summary>
    /// Specifies a target platform.
    /// </summary>
    public enum Platform
    {
        /// <summary>
        /// Windows 64 bit automatic installer.
        /// </summary>
        Win64,

        /// <summary>
        /// Windows 64 bit ZIP package.
        /// </summary>
        Win64Manual,

        /// <summary>
        /// Mac OSX DMG package.
        /// </summary>
        OSX,

        /// <summary>
        /// Linux 64 bit tar.gz package.
        /// </summary>
        Linux64
    }
}
