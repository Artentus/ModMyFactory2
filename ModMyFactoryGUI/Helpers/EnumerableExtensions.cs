//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Collections.Generic;

namespace ModMyFactoryGUI.Helpers
{
    static class EnumerableExtensions
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
    }
}
