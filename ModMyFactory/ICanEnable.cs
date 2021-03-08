//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory
{
    /// <summary>
    /// An object that can be enabled
    /// </summary>
    public interface ICanEnable
    {
        /// <summary>
        /// Raised when the enabled state has changed
        /// </summary>
        event EventHandler EnabledChanged;

        /// <summary>
        /// Whether the object is enabled, disabled or in an undefined state<br/>
        /// The object can never be set to the undefined state
        /// </summary>
        bool? Enabled { get; set; }

        /// <summary>
        /// Whether this object supports disabling
        /// </summary>
        bool CanDisable { get; }
    }
}
