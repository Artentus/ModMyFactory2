namespace ModMyFactory
{
    /// <summary>
    /// Defines which build branch a version is in.
    /// </summary>
    public enum VersionBranch
    {
        /// <summary>
        /// Experimental and in-development release.
        /// </summary>
        Nightly,

        /// <summary>
        /// A public release candidate.
        /// </summary>
        Prerelease,

        /// <summary>
        /// A public release.
        /// </summary>
        Stable,
    }
}
