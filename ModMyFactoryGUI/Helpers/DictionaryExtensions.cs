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
    }
}
