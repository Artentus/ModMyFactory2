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
