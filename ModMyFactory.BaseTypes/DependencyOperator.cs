namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// A list of operators that are valid in a dependency.
    /// </summary>
    public static class DependencyOperator
    {
        public const string None = null;
        public const string Equal = "=";
        public const string LessThan = "<";
        public const string GreaterThan = ">";
        public const string LessThanOrEqual = "<=";
        public const string GreaterThanOrEqual = ">=";
    }
}
