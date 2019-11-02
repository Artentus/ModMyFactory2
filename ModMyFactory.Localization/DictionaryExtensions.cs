using System.Collections.Generic;

namespace ModMyFactory.Localization
{
    static class DictionaryExtensions
    {
        public static void UnionWith<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            foreach (var kvp in second)
                first[kvp.Key] = kvp.Value;
        }
    }
}
