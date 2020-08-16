//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// A list of operators that are valid in a dependency
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
