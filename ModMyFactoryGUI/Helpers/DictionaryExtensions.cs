using System.Collections.Generic;

namespace ModMyFactoryGUI.Helpers
{
    internal static class DictionaryExtensions
    {
        // This function has to be treated with care since it is not given that the reversal of keys and values will yield another valid dictionary
        public static IDictionary<TValue, TKey> Swap<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            var result = new Dictionary<TValue, TKey>();
            foreach (var kvp in dictionary)
                result.Add(kvp.Value, kvp.Key);
            return result;
        }

        public static bool RemoveValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
        {
            bool keyFound = false;
            TKey key = default;
            foreach (var kvp in dictionary)
            {
                if (kvp.Value.Equals(value))
                {
                    keyFound = true;
                    key = kvp.Key;
                    break;
                }
            }

            if (keyFound) dictionary.Remove(key);
            return keyFound;
        }
    }
}
