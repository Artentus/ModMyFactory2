using System.Collections.Generic;

namespace ModMyFactoryGUI.Helpers
{
    internal static class DictionaryExtensions
    {
        public static TValue GetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
#if NETCORE
            return dict.GetValueOrDefault(key, defaultValue);
#else
            if (!dict.TryGetValue(key, out var result))
                result = defaultValue;
            return result;
#endif
        }

        public static bool RemoveEx<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
#if NETCORE
            return dict.Remove(key, out value);
#else
            value = dict.ContainsKey(key) ? dict[key] : default;
            return dict.Remove(key);
#endif
        }

        // This function has to be treated with care since it is not given that the reversal of keys and values will yield another valid dictionary
        public static IDictionary<TValue, TKey> Swap<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            var result = new Dictionary<TValue, TKey>();
            foreach (var kvp in dictionary)
                result.Add(kvp.Value, kvp.Key);
            return result;
        }
    }
}
