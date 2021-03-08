//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections.Generic;

namespace ModMyFactoryGUI.Helpers
{
    internal static class EnumExtensions
    {
        public static T SetFlag<T>(this T value, T flag) where T : Enum
        {
            var intValue = (long)Convert.ChangeType(value, typeof(long));
            var intFlag = (long)Convert.ChangeType(flag, typeof(long));
            return (T)Enum.ToObject(typeof(T), intValue | intFlag);
        }

        public static T UnsetFlag<T>(this T value, T flag) where T : Enum
        {
            var intValue = (long)Convert.ChangeType(value, typeof(long));
            var intFlag = (long)Convert.ChangeType(flag, typeof(long));
            return (T)Enum.ToObject(typeof(T), intValue & ~intFlag);
        }

        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            var arr = Enum.GetValues(typeof(T));
            foreach (var val in arr)
                yield return (T)val;
        }

        public static string? Name<T>(this T value) where T : Enum
            => Enum.GetName(typeof(T), value);
    }
}
