//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ModMyFactoryGUI
{
    internal class ObservableDictionary<TKey, TValue> : NotifyPropertyChangedBase, IDictionary<TKey, TValue>, INotifyCollectionChanged
    {
        public abstract class ObservableDictionaryCollection<T> : NotifyPropertyChangedBase, ICollection<T>, IReadOnlyCollection<T>, INotifyCollectionChanged
        {
            private readonly ObservableDictionary<TKey, TValue> _parent;
            private readonly ICollection<T> _baseCollection;

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public bool IsReadOnly => _baseCollection.IsReadOnly;
            public int Count => _baseCollection.Count;

            protected ObservableDictionaryCollection(ObservableDictionary<TKey, TValue> parent, ICollection<T> baseCollection)
            {
                (_parent, _baseCollection) = (parent, baseCollection);
                _parent.CollectionChanged += OnParentCollectionChanged;
            }

            private IList MapList(IList list)
            {
                var result = new List<T>(list.Count);
                foreach (KeyValuePair<TKey, TValue> kvp in list)
                    result.Add(Map(kvp));
                return result;
            }

            private void OnParentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                IList newItems, oldItems;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        newItems = MapList(e.NewItems);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        oldItems = MapList(e.OldItems);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems));
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        newItems = MapList(e.NewItems);
                        oldItems = MapList(e.OldItems);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems));
                        break;
                }

                if (e.Action != NotifyCollectionChangedAction.Replace)
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            }

            protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
                => CollectionChanged?.Invoke(this, e);

            protected abstract T Map(KeyValuePair<TKey, TValue> kvp);

            public void Add(T item) => _baseCollection.Add(item);

            public bool Remove(T item) => _baseCollection.Remove(item);

            public void Clear() => _baseCollection.Clear();

            public bool Contains(T item) => _baseCollection.Contains(item);

            public void CopyTo(T[] array, int arrayIndex) => _baseCollection.CopyTo(array, arrayIndex);

            public IEnumerator<T> GetEnumerator() => _baseCollection.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public sealed class ObservableKeyCollection : ObservableDictionaryCollection<TKey>
        {
            public ObservableKeyCollection(ObservableDictionary<TKey, TValue> parent, ICollection<TKey> baseCollection)
                : base(parent, baseCollection)
            { }

            protected override TKey Map(KeyValuePair<TKey, TValue> kvp) => kvp.Key;
        }

        public sealed class ObservableValueCollection : ObservableDictionaryCollection<TValue>
        {
            public ObservableValueCollection(ObservableDictionary<TKey, TValue> parent, ICollection<TValue> baseCollection)
                : base(parent, baseCollection)
            { }

            protected override TValue Map(KeyValuePair<TKey, TValue> kvp) => kvp.Value;
        }


        private readonly Dictionary<TKey, TValue> _dictionary;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (_dictionary.ContainsKey(key))
                {
                    _dictionary[key] = value;
                    OnSet(new KeyValuePair<TKey, TValue>(key, value));
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public bool IsReadOnly => false;
        public ObservableKeyCollection Keys { get; }
        public ObservableValueCollection Values { get; }
        public int Count => _dictionary.Count;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        public ObservableDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            Keys = new ObservableKeyCollection(this, _dictionary.Keys);
            Values = new ObservableValueCollection(this, _dictionary.Values);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        protected virtual void OnAdd(KeyValuePair<TKey, TValue> item)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Keys)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Values)));
        }

        protected virtual void OnSet(KeyValuePair<TKey, TValue> item)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Values)));
        }

        protected virtual void OnRemove(KeyValuePair<TKey, TValue> item)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Keys)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Values)));
        }

        protected virtual void OnClear(IList<KeyValuePair<TKey, TValue>> items)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)items));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Keys)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Values)));
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            OnAdd(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool Remove(TKey key)
        {
            bool result = _dictionary.Remove(key, out var value);
            if (result) OnRemove(new KeyValuePair<TKey, TValue>(key, value));
            return result;
        }

        public bool Remove(TKey key, out TValue value)
        {
            bool result = _dictionary.Remove(key, out value);
            if (result) OnRemove(new KeyValuePair<TKey, TValue>(key, value));
            return result;
        }

        public void Clear()
        {
            var items = new KeyValuePair<TKey, TValue>[_dictionary.Count];
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(items, 0);

            _dictionary.Clear();

            OnClear(items);
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Add(item);
            OnAdd(item);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            bool result = ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item);
            if (result) OnRemove(item);
            return result;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
