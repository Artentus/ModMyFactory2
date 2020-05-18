//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections;
using System.Collections.Generic;

namespace ModMyFactoryGUI.Helpers
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        public static IEnumerable<T> AppendFront<T>(this T value, IEnumerable<T> source)
        {
            yield return value;
            foreach (var item in source)
                yield return item;
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            if (comparer == null) comparer = Comparer<TKey>.Default;

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return default(TSource);

                TSource maxElement = enumerator.Current;
                TKey maxKey = selector.Invoke(maxElement);

                while (enumerator.MoveNext())
                {
                    TSource element = enumerator.Current;
                    TKey key = selector.Invoke(element);

                    if (comparer.Compare(key, maxKey) > 0)
                    {
                        maxElement = element;
                        maxKey = key;
                    }
                }

                return maxElement;
            }
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            if (comparer == null) comparer = Comparer<TKey>.Default;

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return default(TSource);

                TSource maxElement = enumerator.Current;
                TKey maxKey = selector.Invoke(maxElement);

                while (enumerator.MoveNext())
                {
                    TSource element = enumerator.Current;
                    TKey key = selector.Invoke(element);

                    if (comparer.Compare(key, maxKey) < 0)
                    {
                        maxElement = element;
                        maxKey = key;
                    }
                }

                return maxElement;
            }
        }

        public static IEnumerable<T> FilterByType<T>(this IEnumerable collection)
        {
            foreach (var item in collection)
            {
                if (item is T filtered)
                    yield return filtered;
            }
        }
    }
}
