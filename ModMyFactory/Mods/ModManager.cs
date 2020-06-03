//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// Manages mods for one Factorio version
    /// </summary>
    public sealed class ModManager : ICollection<Mod>, IReadOnlyCollection<Mod>, INotifyCollectionChanged
    {
        private readonly Dictionary<string, ModFamily> _families;
        private volatile bool _clearing = false;
        internal static readonly AccurateVersion FormatSwitch = new AccurateVersion(0, 17); // Native support for multiple mods in Factorio 0.17 and onwards

        /// <summary>
        /// Is raised if mods in a family change their enabled state
        /// </summary>
        public event EventHandler<ModFamilyEnabledChangedEventArgs> FamilyEnabledChanged;

        /// <summary>
        /// Occurs when the mod collection changes
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// The version of Factorio being managed
        /// </summary>
        public AccurateVersion FactorioVersion { get; }

        /// <summary>
        /// A collection of mod families formed by the managed mods
        /// </summary>
        public IReadOnlyCollection<ModFamily> Families { get; }

        /// <summary>
        /// The number of mods managed
        /// </summary>
        public int Count => Families.Sum(family => family.Count());

        bool ICollection<Mod>.IsReadOnly => false;

        /// <param name="factorioVersion">
        /// The version of Factorio getting managed<br/>
        /// Only considers major version
        /// </param>
        public ModManager(AccurateVersion factorioVersion)
        {
            _families = new Dictionary<string, ModFamily>();
            Families = _families.Values;

            FactorioVersion = factorioVersion.ToMajor();
        }

        private void OnFamilyModsEnabledChanged(object sender, EventArgs e)
        {
            var family = (ModFamily)sender;
            FamilyEnabledChanged?.Invoke(this, new ModFamilyEnabledChangedEventArgs(family));
        }

        private ModFamily GetFamily(string name)
        {
            if (!_families.TryGetValue(name, out var result))
            {
                result = new ModFamily(name);
                _families.Add(name, result);
                result.CollectionChanged += OnFamilyCollectionChanged;
                result.ModsEnabledChanged += OnFamilyModsEnabledChanged;
            }
            return result;
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        private void OnFamilyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (!_clearing) // Ignore if we are clearing globally
                {
                    // We need to propagate a reset event as multiple remove events since clearing a single family does not clear the entire manager
                    foreach (Mod mod in e.OldItems)
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, mod));
                }
            }
            else
            {
                OnCollectionChanged(e);
            }
        }

        /// <summary>
        /// Tries to get the mod family with the specified name
        /// </summary>
        public bool TryGetFamily(string name, out ModFamily result) => _families.TryGetValue(name, out result);

        /// <summary>
        /// Adds a mod to be managed
        /// </summary>
        public void Add(Mod mod)
        {
            if (mod is null) throw new ArgumentNullException();
            if (mod.FactorioVersion.ToMajor() != FactorioVersion) throw new ArgumentException("Mod has incorrect Factorio version.");

            var family = GetFamily(mod.Name);
            if (family.Contains(mod)) return; // If mod already managed do nothing

            family.Add(mod);
        }

        /// <summary>
        /// Removes a mod from the manager
        /// </summary>
        public bool Remove(Mod mod)
        {
            if (mod is null) return false;
            if (!TryGetFamily(mod.Name, out var family)) return false;

            var result = family.Remove(mod);
            if (result)
            {
                if (family.Count == 0)
                {
                    family.CollectionChanged -= OnFamilyCollectionChanged;
                    family.ModsEnabledChanged -= OnFamilyModsEnabledChanged;
                    _families.Remove(family.FamilyName);
                }
            }
            return result;
        }

        /// <summary>
        /// Removes all managed mods
        /// </summary>
        public void Clear()
        {
            _clearing = true;

            foreach (var family in Families)
            {
                family.CollectionChanged -= OnFamilyCollectionChanged;
                family.ModsEnabledChanged -= OnFamilyModsEnabledChanged;
            }
            _families.Clear();

            // Needs to be called manually since the event hook can't handle it
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            _clearing = false;
        }

        /// <summary>
        /// Checks if a mod is managed by this manager
        /// </summary>
        public bool Contains(Mod mod)
        {
            if (mod is null) return false;
            if (!TryGetFamily(mod.Name, out var family)) return false;
            return family.Contains(mod);
        }

        /// <summary>
        /// Checks if a mod with the specified name and version is managed by this manager
        /// </summary>
        public bool Contains(string name, AccurateVersion version)
        {
            if (TryGetFamily(name, out var family))
                return family.Contains(version);

            return false;
        }

        /// <summary>
        /// Checks if a mod with the specified name and version is managed by this manager
        /// </summary>
        public bool Contains(string name, AccurateVersion version, out Mod mod)
        {
            if (TryGetFamily(name, out var family))
                return family.Contains(version, out mod);

            mod = null;
            return false;
        }

        /// <summary>
        /// Checks if a family with the specified name is managed by this manager
        /// </summary>
        public bool Contains(string name)
            => TryGetFamily(name, out _);

        /// <summary>
        /// Checks if a family with the specified name is managed by this manager
        /// </summary>
        public bool Contains(string name, out ModFamily family)
            => TryGetFamily(name, out family);

        public IEnumerator<Mod> GetEnumerator()
        {
            foreach (var family in Families)
            {
                foreach (var mod in family)
                    yield return mod;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<Mod>.CopyTo(Mod[] array, int arrayIndex) => throw new NotSupportedException();
    }
}
