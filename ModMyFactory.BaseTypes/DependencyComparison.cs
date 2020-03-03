//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// Implements comparison behavior of a dependency.
    /// </summary>
    public class DependencyComparison
    {
        private readonly Func<AccurateVersion, AccurateVersion, bool> comparisonFunction;

        /// <summary>
        /// The operator used for the comparison.
        /// </summary>
        public virtual string Operator { get; }

        public DependencyComparison(string op)
        {
            comparisonFunction = op switch
            {
                DependencyOperator.None => (x, y) => true,
                DependencyOperator.Equal => (x, y) => x.CompareTo(y) == 0,
                DependencyOperator.LessThan => (x, y) => x.CompareTo(y) < 0,
                DependencyOperator.GreaterThan => (x, y) => x.CompareTo(y) > 0,
                DependencyOperator.LessThanOrEqual => (x, y) => x.CompareTo(y) <= 0,
                DependencyOperator.GreaterThanOrEqual => (x, y) => x.CompareTo(y) >= 0,
                _ => throw new ArgumentException("Unknown operator", nameof(op)),
            };
            Operator = op;
        }

        /// <summary>
        /// Applies the comparison operator to a mods version and the dependency test version.
        /// </summary>
        protected internal virtual bool TestFor(AccurateVersion modVersion, AccurateVersion testVersion) => comparisonFunction(modVersion, testVersion);

        public override string ToString() => Operator;
    }
}
