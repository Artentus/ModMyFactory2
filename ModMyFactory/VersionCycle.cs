namespace ModMyFactory
{
    /// <summary>
    /// Defines which build cycle a version is in.
    /// </summary>
    public enum VersionCycle
    {
        /// <summary>
        /// An alpha build. Not feature complete.
        /// </summary>
        Alpha = 0,

        /// <summary>
        /// A beta build. Core features complete but unstable.
        /// </summary>
        Beta = 1,

        /// <summary>
        /// A release build. Core feature complete and stable.
        /// </summary>
        Release = 2,
    }
}
