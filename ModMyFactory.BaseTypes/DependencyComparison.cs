using System;

namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// Implements comparison behavior of a dependency.
    /// </summary>
    public class DependencyComparison
    {
        readonly Func<AccurateVersion, AccurateVersion, bool> comparisonFunction;

        /// <summary>
        /// The operator used for the comparison.
        /// </summary>
        public virtual string Operator { get; }

        public DependencyComparison(string op)
        {
            switch (op)
            {
                case DependencyOperator.None:
                    comparisonFunction = (x, y) => true;
                    break;
                case DependencyOperator.Equal:
                    comparisonFunction = (x, y) => x.CompareTo(y) == 0;
                    break;
                case DependencyOperator.LessThan:
                    comparisonFunction = (x, y) => x.CompareTo(y) < 0;
                    break;
                case DependencyOperator.GreaterThan:
                    comparisonFunction = (x, y) => x.CompareTo(y) > 0;
                    break;
                case DependencyOperator.LessThanOrEqual:
                    comparisonFunction = (x, y) => x.CompareTo(y) <= 0;
                    break;
                case DependencyOperator.GreaterThanOrEqual:
                    comparisonFunction = (x, y) => x.CompareTo(y) >= 0;
                    break;
                default:
                    throw new ArgumentException();
            }
            Operator = op;
        }

        /// <summary>
        /// Applies the comparison operator to a mods version and the dependency test version.
        /// </summary>
        protected internal virtual bool TestFor(AccurateVersion modVersion, AccurateVersion testVersion) => comparisonFunction(modVersion, testVersion);

        public override string ToString() => Operator;
    }
}
