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
using System.ComponentModel;
using System.Linq;

namespace ModMyFactoryGUI
{
    // Not as powerful as the CollectionView in WPF but sorting and filtering is all we need
    internal class CollectionView<T> : NotifyPropertyChangedBase, ICollection<T>, IReadOnlyCollection<T>, INotifyCollectionChanged, IDisposable
    {
        private readonly ICollection<T> _baseCollection;
        private readonly INotifyCollectionChanged? _baseCollectionChanged;
        private IComparer<T>? _comparer;
        private Func<T, bool>? _filter;
        private ICollection<T> _evaluated;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public bool IsReadOnly => true;

        public IComparer<T>? Comparer
        {
            get => _comparer;
            set
            {
                if (value != _comparer)
                {
                    _comparer = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Comparer)));

                    Refresh();
                }
            }
        }

        public Func<T, bool>? Filter
        {
            get => _filter;
            set
            {
                if (value != _filter)
                {
                    _filter = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Filter)));

                    Refresh();
                }
            }
        }

        public int Count => _evaluated.Count;

        public CollectionView(in ICollection<T> baseCollection, in IComparer<T>? comparer, in Func<T, bool>? filter)
        {
            if (baseCollection is null) throw new ArgumentNullException(nameof(baseCollection));
            (_baseCollection, _comparer, _filter) = (baseCollection, comparer, filter);

            _baseCollectionChanged = _baseCollection as INotifyCollectionChanged;
            if (!(_baseCollectionChanged is null)) _baseCollectionChanged.CollectionChanged += BaseCollectionChangedHandler;

            _evaluated = Evaluate(_baseCollection);
        }

        public CollectionView(in ICollection<T> baseCollection, in IComparer<T>? comparer)
            : this(baseCollection, comparer, null)
        { }

        public CollectionView(in ICollection<T> baseCollection, in Comparison<T> comparison, in Func<T, bool>? filter)
            : this(baseCollection, Comparer<T>.Create(comparison), filter)
        { }

        public CollectionView(in ICollection<T> baseCollection, in Comparison<T> comparison)
            : this(baseCollection, comparison, null)
        { }

        public CollectionView(in ICollection<T> baseCollection, in Func<T, bool>? filter)
            : this(baseCollection, default(IComparer<T>?), filter)
        { }

        public CollectionView(in ICollection<T> baseCollection)
            : this(baseCollection, default(IComparer<T>?), null)
        { }

        private void BaseCollectionChangedHandler(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        protected virtual ICollection<T> Evaluate(ICollection<T> baseCollection)
        {
            IEnumerable<T> filtered = _filter is null
                ? baseCollection
                : baseCollection.Where(item => _filter(item));

            var sorted = new List<T>(filtered);
            if (!(_comparer is null)) sorted.Sort(_comparer);

            return sorted;
        }

        public void Refresh()
        {
            var old = _evaluated.ToArray();
            _evaluated = Evaluate(_baseCollection);

            var removed = old.Except(_evaluated).ToArray();
            if (removed.Length > 0) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, 0));

            var added = _evaluated.Except(old).ToArray();
            if (added.Length > 0) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added, 0));

            if (old.Length != _evaluated.Count)
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        public void Add(T item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Remove(T item) => throw new NotSupportedException();

        public bool Contains(T item) => _baseCollection.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _evaluated.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _evaluated.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        #region IDisposable Support

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (!(_baseCollectionChanged is null)) _baseCollectionChanged.CollectionChanged -= BaseCollectionChangedHandler;
                    if (_baseCollection is IDisposable disposable) disposable.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose() => Dispose(true);

        #endregion IDisposable Support
    }
}
