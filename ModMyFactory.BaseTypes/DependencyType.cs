namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// SPecifies the type of a dependency.
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// A normal dependency.
        /// </summary>
        Normal,

        /// <summary>
        /// An inverted dependency.
        /// </summary>
        Inverted,

        /// <summary>
        /// An optional dependency.
        /// </summary>
        Optional,

        /// <summary>
        /// A hidden dependency.
        /// </summary>
        Hidden
    }
}
