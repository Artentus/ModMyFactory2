using System;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// An event that occurs if the endabled states of mods in a family change.
    /// </summary>
    public class ModFamilyEnabledChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The family that raised the event originally.
        /// </summary>
        public ModFamily Family { get; }

        public ModFamilyEnabledChangedEventArgs(ModFamily family)
        {
            Family = family;
        }
    }
}
