using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// Groups mods with the same name but different version.
    /// </summary>
    public sealed class ModFamily : ICollection<Mod>
    {
        readonly List<Mod> _mods;

        /// <summary>
        /// The shared name of this mod family.
        /// </summary>
        public string FamilyName { get; }

        /// <summary>
        /// The number of mods in this family.
        /// </summary>
        public int Count => _mods.Count;

        bool ICollection<Mod>.IsReadOnly => false;

        public ModFamily(string familyName)
        {
            _mods = new List<Mod>();
            FamilyName = familyName;
        }

        public ModFamily(Mod mod)
            : this(mod?.Name)
        {
            if (mod is null) throw new ArgumentNullException();
            _mods.Add(mod);
        }

        /// <summary>
        /// Adds a mod to the family.
        /// </summary>
        public void Add(Mod item)
        {
            if (item is null) throw new ArgumentNullException();
            if (!string.Equals(item.Name, FamilyName, StringComparison.InvariantCulture)) throw new ArgumentException("The mod is not part of this family.");
            _mods.Add(item);
        }

        /// <summary>
        /// Removes a mod from the family.
        /// </summary>
        public bool Remove(Mod item) => _mods.Remove(item);

        /// <summary>
        /// Removes all mods from the family.
        /// </summary>
        public void Clear() => _mods.Clear();

        /// <summary>
        /// Checks if a mod is contained in this family.
        /// </summary>
        public bool Contains(Mod item) => !(item is null) && string.Equals(item.Name, FamilyName, StringComparison.InvariantCulture) && _mods.Contains(item);

        void ICollection<Mod>.CopyTo(Mod[] array, int arrayIndex) => _mods.CopyTo(array, arrayIndex);

        public IEnumerator<Mod> GetEnumerator() => _mods.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _mods.GetEnumerator();
    }
}
