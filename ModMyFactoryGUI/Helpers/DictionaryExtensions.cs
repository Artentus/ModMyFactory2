//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Collections.Generic;

namespace ModMyFactoryGUI.Helpers
{
    internal static class DictionaryExtensions
    {
        // This function has to be treated with care since it is not given that the reversal of keys and values will yield another valid dictionary
        public static IDictionary<TValue, TKey> Swap<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
            where TKey : notnull
            where TValue : notnull
        {
            var result = new Dictionary<TValue, TKey>();
            foreach (var kvp in dictionary)
                result.Add(kvp.Value, kvp.Key);
            return result;
        }

        public static bool RemoveValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
        {
            bool keyFound = false;
            TKey? key = default;
            foreach (var kvp in dictionary)
            {
                if (object.Equals(kvp.Value, value))
                {
                    key = kvp.Key;
                    keyFound = true;
                    break;
                }
            }

            if (keyFound) dictionary.Remove(key!);
            return keyFound;
        }
    }
}
