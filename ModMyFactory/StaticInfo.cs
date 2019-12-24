namespace ModMyFactory
{
    /// <summary>
    /// Information about ModMyFactory.
    /// </summary>
    public static class StaticInfo
    {
        /// <summary>
        /// Full version of ModMyFactory.
        /// </summary>
        public static CustomVersion Version { get; } = new CustomVersion(2, 1, 0, 0, 1, VersionCycle.Alpha, VersionBranch.Nightly);
    }
}
