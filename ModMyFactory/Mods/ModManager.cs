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
using System.Linq;

namespace ModMyFactory.Mods
{
    public sealed class ModManager : ICollection<Mod>
    {
        internal static readonly AccurateVersion FormatSwitch = new AccurateVersion(0, 17); // Native support for multiple mods in Factorio 0.17 and onwards

        readonly Dictionary<string, ModFamily> _families;

        /// <summary>
        /// Is raised if mods in a family change their enabled state.
        /// </summary>
        public event EventHandler<ModFamilyEnabledChangedEventArgs> FamilyEnabledChanged;

        /// <summary>
        /// The version of Factorio being managed.
        /// </summary>
        public AccurateVersion FactorioVersion { get; }

        /// <summary>
        /// A collection of mod families formed by the managed mods.
        /// </summary>
        public IReadOnlyCollection<ModFamily> Families { get; }

        /// <summary>
        /// The number of mods managed.
        /// </summary>
        public int Count => Families.Sum(family => family.Count());

        /// <param name="factorioVersion">The version of Factorio getting managed. Only considers major version.</param>
        public ModManager(AccurateVersion factorioVersion)
        {
            _families = new Dictionary<string, ModFamily>();
            FactorioVersion = factorioVersion.ToMajor();
            Families = _families.Values;
        }

        void OnFamilyModsEnabledChanged(object sender, EventArgs e)
        {
            var family = (ModFamily)sender;
            FamilyEnabledChanged?.Invoke(this, new ModFamilyEnabledChangedEventArgs(family));
        }

        ModFamily GetFamily(string name)
        {
            if (!_families.TryGetValue(name, out var result))
            {
                result = new ModFamily(name);
                _families.Add(name, result);
                result.ModsEnabledChanged += OnFamilyModsEnabledChanged;
            }
            return result;
        }

        bool TryGetFamily(string name, out ModFamily result) => _families.TryGetValue(name, out result);

        /// <summary>
        /// Adds a mod to be managed.
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
        /// Removes a mod from the manager.
        /// </summary>
        public bool Remove(Mod mod)
        {
            if (mod is null) return false;
            if (!TryGetFamily(mod.Name, out var family)) return false;

            var result = family.Remove(mod);
            if (result && (family.Count == 0))
            {
                family.ModsEnabledChanged -= OnFamilyModsEnabledChanged;
                _families.Remove(family.FamilyName);
            }
            return result;
        }

        /// <summary>
        /// Removes all managed mods.
        /// </summary>
        public void Clear()
        {
            foreach (var family in Families)
                family.ModsEnabledChanged -= OnFamilyModsEnabledChanged;
            _families.Clear();
        }

        /// <summary>
        /// Checks if a mod is managed by this manager.
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

        bool ICollection<Mod>.IsReadOnly => false;

        void ICollection<Mod>.CopyTo(Mod[] array, int arrayIndex) => throw new NotSupportedException();
    }
}
