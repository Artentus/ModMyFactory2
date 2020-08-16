//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// Specifies the type of a dependency
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// A normal dependency
        /// </summary>
        Normal,

        /// <summary>
        /// An inverted dependency
        /// </summary>
        Inverted,

        /// <summary>
        /// An optional dependency
        /// </summary>
        Optional,

        /// <summary>
        /// A hidden dependency
        /// </summary>
        Hidden
    }
}
