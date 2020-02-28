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
