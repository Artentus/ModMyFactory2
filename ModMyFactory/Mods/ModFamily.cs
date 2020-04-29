//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// Groups mods with the same name but different version.
    /// Only one mod in a family can be enabled at a time.
    /// </summary>
    public sealed class ModFamily : ICollection<Mod>, INotifyCollectionChanged
    {
        private readonly List<Mod> _mods;
        private Mod _enabledMod;
        private volatile bool _raiseEvent = true;

        /// <summary>
        /// Is raised if the enabled states of the mods in the family change
        /// </summary>
        public event EventHandler ModsEnabledChanged;

        /// <summary>
        /// Occurs when the mods in the family change
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// The shared name of this mod family
        /// </summary>
        public string FamilyName { get; }

        /// <summary>
        /// The currently enabled mod of this family, or null if no mod is enabled
        /// </summary>
        public Mod EnabledMod => _enabledMod;

        /// <summary>
        /// The number of mods in this family
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
            Add(mod);
        }

        private void OnModEnabledChanged(object sender, EventArgs e)
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

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        /// <summary>
        /// Adds a mod to the family
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

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, mod));
        }

        /// <summary>
        /// Removes a mod from the family
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

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, mod));
            }
            return result;
        }

        /// <summary>
        /// Removes all mods from the family
        /// </summary>
        public void Clear()
        {
            foreach (var mod in _mods)
            {
                mod.Family = null;
                mod.EnabledChanged -= OnModEnabledChanged;
            }

            var listCopy = new Mod[_mods.Count];
            _mods.CopyTo(listCopy);

            _enabledMod = null;
            _mods.Clear();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, listCopy));
        }

        /// <summary>
        /// Checks if a mod is contained in this family
        /// </summary>
        public bool Contains(Mod item) => !(item is null) && string.Equals(item.Name, FamilyName, StringComparison.InvariantCulture) && _mods.Contains(item);

        public IEnumerator<Mod> GetEnumerator() => _mods.GetEnumerator();

        void ICollection<Mod>.CopyTo(Mod[] array, int arrayIndex) => _mods.CopyTo(array, arrayIndex);

        IEnumerator IEnumerable.GetEnumerator() => _mods.GetEnumerator();
    }
}
