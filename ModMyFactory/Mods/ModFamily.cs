using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// Groups mods with the same name but different version.
    /// Only one mod in a family can be enabled at a time.
    /// </summary>
    public sealed class ModFamily : ICollection<Mod>
    {
        readonly List<Mod> _mods;
        Mod _enabledMod;
        volatile bool _raiseEvent = true;

        /// <summary>
        /// Is raised if the enabled states of the mods in the family change.
        /// </summary>
        public event EventHandler ModsEnabledChanged;

        /// <summary>
        /// The shared name of this mod family.
        /// </summary>
        public string FamilyName { get; }

        /// <summary>
        /// The number of mods in this family.
        /// </summary>
        public int Count => _mods.Count;

        /// <summary>
        /// The currently enabled mod of this family, or null if no mod is enabled.
        /// </summary>
        public Mod EnabledMod => _enabledMod;

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
            Add(mod);
        }

        void OnModEnabledChanged(object sender, EventArgs e)
        {
            var senderMod = (Mod)sender;
            if (senderMod.Enabled)
            {
                _raiseEvent = false;
                foreach (var mod in _mods)
                {
                    if ((mod != senderMod) && mod.CanDisable)
                        mod.Enabled = false;
                }
                _raiseEvent = true;

                _enabledMod = senderMod;
            }
            else if (senderMod == _enabledMod)
                _enabledMod = null;

            if (_raiseEvent) ModsEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds a mod to the family.
        /// </summary>
        public void Add(Mod mod)
        {
            if (mod is null) throw new ArgumentNullException();
            if (!string.Equals(mod.Name, FamilyName, StringComparison.InvariantCulture)) throw new ArgumentException("The mod is not part of this family.");
            if (!(mod.Family is null)) throw new InvalidOperationException("The mod is already part of a family.");

            mod.Family = this;
            _mods.Add(mod);

            if (mod.Enabled)
            {
                if (_enabledMod == null)
                    _enabledMod = mod;
                else
                    mod.Enabled = false;
            }
            mod.EnabledChanged += OnModEnabledChanged;
        }

        /// <summary>
        /// Removes a mod from the family.
        /// </summary>
        public bool Remove(Mod mod)
        {
            if (mod is null) return false;

            bool result = _mods.Remove(mod);
            if (result)
            {
                mod.Family = null;
                mod.EnabledChanged -= OnModEnabledChanged;
                if (mod == _enabledMod) _enabledMod = null;
            }
            return result;
        }

        /// <summary>
        /// Removes all mods from the family.
        /// </summary>
        public void Clear()
        {
            foreach (var mod in _mods)
            {
                mod.Family = null;
                mod.EnabledChanged -= OnModEnabledChanged;
            }

            _enabledMod = null;
            _mods.Clear();
        }

        /// <summary>
        /// Checks if a mod is contained in this family.
        /// </summary>
        public bool Contains(Mod item) => !(item is null) && string.Equals(item.Name, FamilyName, StringComparison.InvariantCulture) && _mods.Contains(item);

        void ICollection<Mod>.CopyTo(Mod[] array, int arrayIndex) => _mods.CopyTo(array, arrayIndex);

        public IEnumerator<Mod> GetEnumerator() => _mods.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _mods.GetEnumerator();
    }
}
