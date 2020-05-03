using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ModMyFactoryGUI
{
    internal static class CollectionExtensions
    {
        private sealed class SortedObservableCollection<T> : NotifyPropertyChangedBase, ICollection<T>, IReadOnlyCollection<T>, INotifyCollectionChanged
        {
            private readonly ICollection<T> _baseCollection;
            private readonly IComparer<T> _comparer;

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public int Count => _baseCollection.Count;

            bool ICollection<T>.IsReadOnly => true;

            public SortedObservableCollection(ICollection<T> baseCollection, IComparer<T> comparer)
            {
                (_baseCollection, _comparer) = (baseCollection, comparer);

                var observable1 = (INotifyCollectionChanged)baseCollection;
                observable1.CollectionChanged += OnBaseCollectionChanged;
                var observable2 = (INotifyPropertyChanged)baseCollection;
                observable2.PropertyChanged += OnBasePropertyChanged;
            }

            ~SortedObservableCollection()
            {
                var observable1 = (INotifyCollectionChanged)_baseCollection;
                observable1.CollectionChanged -= OnBaseCollectionChanged;
                var observable2 = (INotifyPropertyChanged)_baseCollection;
                observable2.PropertyChanged -= OnBasePropertyChanged;
            }

            private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
                => CollectionChanged?.Invoke(this, e);

            private void OnBaseCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                => OnCollectionChanged(e);

            private void OnBasePropertyChanged(object sender, PropertyChangedEventArgs e)
                => OnPropertyChanged(e);

            public void Add(T item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Remove(T item) => throw new NotSupportedException();

            public bool Contains(T item) => _baseCollection.Contains(item);

            public void CopyTo(T[] array, int arrayIndex)
            {
                using var enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    array[arrayIndex] = enumerator.Current;
                    arrayIndex++;
                }
            }

            public void Refresh()
                => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this.ToList()));

            public IEnumerator<T> GetEnumerator()
                => _baseCollection.OrderBy(item => item, _comparer).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static ICollection<T> ToSorted<T, TCollection>(this TCollection collection, IComparer<T> comparer)
            where TCollection : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
            => new SortedObservableCollection<T>(collection, comparer);

        public static ICollection<T> ToSorted<T, TCollection>(this TCollection collection, Comparison<T> comparison)
            where TCollection : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
            => new SortedObservableCollection<T>(collection, Comparer<T>.Create(comparison));

        public static ICollection<T> ToSorted<T, TCollection>(this TCollection collection)
            where TCollection : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
            => new SortedObservableCollection<T>(collection, Comparer<T>.Default);
    }
}
