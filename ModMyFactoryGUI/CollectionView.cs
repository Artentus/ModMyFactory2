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
        private readonly INotifyCollectionChanged _baseCollectionChanged;
        private IComparer<T> _comparer;
        private Func<T, bool> _filter;
        private ICollection<T> _evaluated;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool IsReadOnly => true;

        public IComparer<T> Comparer
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

        public Func<T, bool> Filter
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

        public CollectionView(ICollection<T> baseCollection, IComparer<T> comparer, Func<T, bool> filter)
        {
            if (baseCollection is null) throw new ArgumentNullException(nameof(baseCollection));
            (_baseCollection, _comparer, _filter) = (baseCollection, comparer, filter);

            _baseCollectionChanged = _baseCollection as INotifyCollectionChanged;
            if (!(_baseCollectionChanged is null)) _baseCollectionChanged.CollectionChanged += BaseCollectionChangedHandler;

            _evaluated = Evaluate(_baseCollection);
        }

        public CollectionView(ICollection<T> baseCollection, IComparer<T> comparer)
            : this(baseCollection, comparer, null)
        { }

        public CollectionView(ICollection<T> baseCollection, Comparison<T> comparison, Func<T, bool> filter)
            : this(baseCollection, Comparer<T>.Create(comparison), filter)
        { }

        public CollectionView(ICollection<T> baseCollection, Comparison<T> comparison)
            : this(baseCollection, comparison, null)
        { }

        public CollectionView(ICollection<T> baseCollection, Func<T, bool> filter)
            : this(baseCollection, default(IComparer<T>), filter)
        { }

        public CollectionView(ICollection<T> baseCollection)
            : this(baseCollection, default(IComparer<T>), null)
        { }

        private void BaseCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e) => Refresh();

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        protected virtual ICollection<T> Evaluate(ICollection<T> baseCollection)
        {
            IEnumerable<T> filtered = Filter is null
                ? baseCollection
                : baseCollection.Where(item => Filter(item));

            var sorted = new List<T>(filtered);
            if (!(Comparer is null)) sorted.Sort(Comparer);

            return sorted;
        }

        public void Refresh()
        {
            _evaluated = Evaluate(_baseCollection);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
