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
        private readonly List<ICanEnable> _mods;
        private bool? _enabled;

        /// <summary>
        /// Raised when the enabled state of the modpack changes
        /// </summary>
        public event EventHandler EnabledChanged;

        /// <summary>
        /// Whether the modpack is enabled
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
        /// Unique identifier of this modpack
        /// Only use locally, not across machines
        /// </summary>
        public Guid Uid { get; }

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

        public Modpack(Guid uid)
            => Uid = uid;

        public Modpack()
            : this(Guid.NewGuid())
        { }

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

        private void AssertCanAdd(Modpack pack)
        {
            if (pack == this) throw new InvalidOperationException("Cannot create circular modpack references");

            foreach (var item in pack)
                AssertCanAdd(item);
        }

        private void AssertCanAdd(ICanEnable item)
        {
            if (item is Modpack pack) AssertCanAdd(pack);
        }

        protected virtual void OnEnabledChanged(EventArgs e)
            => EnabledChanged?.Invoke(this, e);

        public virtual void Add(ICanEnable item)
        {
            AssertCanAdd(item);

            _mods.Add(item);
            item.EnabledChanged += OnModEnabledChanged;
            EvaluateEnabledState();
        }

        public virtual void AddRange(IEnumerable<ICanEnable> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
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
