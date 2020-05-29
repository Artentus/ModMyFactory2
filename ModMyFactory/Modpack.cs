//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Mods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactory
{
    /// <summary>
    /// Groups mods and other modpacks
    /// </summary>
    public class Modpack : ICanEnable, ICollection<ICanEnable>
    {
        private readonly List<ICanEnable> _mods = new List<ICanEnable>();
        private bool? _enabled;

        /// <summary>
        /// Raised when the enabled state of the modpack changes
        /// </summary>
        public event EventHandler EnabledChanged;

        /// <summary>
        /// Whether the modpack is enabled<br/>
        /// Undefined if mods in the modpack have different enabled states
        /// </summary>
        public bool? Enabled
        {
            get => _enabled;
            set
            {
                if (value is null) throw new InvalidOperationException("Cannot set enabled state to null");
                else SetEnabledState(value.Value);
            }
        }

        /// <summary>
        /// The modpacks name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The mods in this modpack
        /// </summary>
        public IEnumerable<Mod> Mods => this.Where(i => i is Mod).Cast<Mod>();

        /// <summary>
        /// The modpacks in this modpack
        /// </summary>
        public IEnumerable<Modpack> Modpacks => this.Where(i => i is Modpack).Cast<Modpack>();

        bool ICanEnable.CanDisable => true;

        public int Count => _mods.Count;

        bool ICollection<ICanEnable>.IsReadOnly => false;

        private void SetEnabledState(bool enabled)
        {
            if (_enabled.Value != enabled)
            {
                _enabled = enabled;
                _mods.ForEach(m => m.Enabled = enabled);
                OnEnabledChanged(EventArgs.Empty);
            }
        }

        private void EvaluateEnabledState()
        {
            bool? newState;
            if (_mods.Count == 0)
            {
                newState = false;
            }
            else
            {
                var relevantMods = _mods.Where(m => m.CanDisable); // Don't evaluate mods that cannot be disabled
                if (relevantMods.Count() == 0)
                {
                    newState = true; // All mods in the pack are always enabled
                }
                else
                {
                    newState = relevantMods.First().Enabled;
                    if (!relevantMods.All(m => m.Enabled == newState)) newState = null;
                }
            }

            if (newState != _enabled)
            {
                _enabled = newState;
                OnEnabledChanged(EventArgs.Empty);
            }
        }

        private void OnModEnabledChanged(object sender, EventArgs e)
            => EvaluateEnabledState();

        private bool CanAdd(Modpack pack)
        {
            if (pack == this) return false;

            foreach (var item in pack)
                if (!CanAdd(item)) return false;

            return true;
        }

        private bool CanAdd(ICanEnable item)
        {
            if (item is Modpack pack) return CanAdd(pack);
            return true;
        }

        private void AssertCanAdd(ICanEnable item)
        {
            if (!CanAdd(item)) throw new InvalidOperationException("Cannot create circular modpack references");
        }

        private bool AddInternal(ICanEnable item, bool safe)
        {
            if (!_mods.Contains(item))
            {
                if (safe)
                {
                    if (CanAdd(item))
                    {
                        _mods.Add(item);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    AssertCanAdd(item);

                    _mods.Add(item);
                    return true;
                }
            }

            return false;
        }

        protected virtual void OnEnabledChanged(EventArgs e)
            => EnabledChanged?.Invoke(this, e);

        /// <summary>
        /// Adds a mod or another modpack to the modpack<br/>
        /// If the add operation would result in a circular reference an exception is thrown
        /// </summary>
        public virtual void Add(ICanEnable item)
        {
            if (AddInternal(item, false))
            {
                item.EnabledChanged += OnModEnabledChanged;
                EvaluateEnabledState();
            }
        }

        /// <summary>
        /// Adds a mod or another modpack to the modpack<br/>
        /// If the add operation would result in a circular reference it is ignored
        /// </summary>
        public virtual void AddSafe(ICanEnable item)
        {
            if (AddInternal(item, true))
            {
                item.EnabledChanged += OnModEnabledChanged;
                EvaluateEnabledState();
            }
        }

        /// <summary>
        /// Adds a collection of mods and other modpacks to the modpack<br/>
        /// If an add operation would result in a circular reference an exception is thrown
        /// </summary>
        public virtual void AddRange(IEnumerable<ICanEnable> collection)
        {
            foreach (var item in collection)
            {
                if (AddInternal(item, false))
                    item.EnabledChanged += OnModEnabledChanged;
            }
            EvaluateEnabledState();
        }

        /// <summary>
        /// Adds a collection of mods and other modpacks to the modpack<br/>
        /// If an add operation would result in a circular reference it is ignored
        /// </summary>
        public virtual void AddRangeSafe(IEnumerable<ICanEnable> collection)
        {
            foreach (var item in collection)
            {
                if (AddInternal(item, true))
                    item.EnabledChanged += OnModEnabledChanged;
            }
            EvaluateEnabledState();
        }

        public virtual bool Remove(ICanEnable item)
        {
            item.EnabledChanged -= OnModEnabledChanged;
            bool result = _mods.Remove(item);
            EvaluateEnabledState();
            return result;
        }

        public virtual void Clear()
        {
            _mods.ForEach(m => m.EnabledChanged -= OnModEnabledChanged);
            _mods.Clear();
            EvaluateEnabledState();
        }

        public bool Contains(ICanEnable item)
            => _mods.Contains(item);

        public void CopyTo(ICanEnable[] array, int arrayIndex)
            => _mods.CopyTo(array, arrayIndex);

        public IEnumerator<ICanEnable> GetEnumerator() => _mods.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
