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
    public sealed class ModManager : ICollection<Mod>, INotifyCollectionChanged
    {
        private readonly Dictionary<string, ModFamily> _families;
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

        /// <param name="factorioVersion">The version of Factorio getting managed. Only considers major version.</param>
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
                result.ModsEnabledChanged += OnFamilyModsEnabledChanged;
            }
            return result;
        }

        private bool TryGetFamily(string name, out ModFamily result) => _families.TryGetValue(name, out result);

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
                    => CollectionChanged?.Invoke(this, e);

        /// <summary>
        /// Adds a mod to be managed
        /// </summary>
        public void Add(Mod mod)
        {
            // ToDo: check directory

            if (mod is null) throw new ArgumentNullException();
            if (mod.FactorioVersion.ToMajor() != FactorioVersion) throw new ArgumentException("Mod has incorrect Factorio version.");

            var family = GetFamily(mod.Name);
            if (family.Contains(mod)) return; // If mod already managed do nothing

            family.Add(mod);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, mod));
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
                    family.ModsEnabledChanged -= OnFamilyModsEnabledChanged;
                    _families.Remove(family.FamilyName);
                }

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, mod));
            }
            return result;
        }

        /// <summary>
        /// Removes all managed mods
        /// </summary>
        public void Clear()
        {
            foreach (var family in Families)
                family.ModsEnabledChanged -= OnFamilyModsEnabledChanged;
            _families.Clear();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
