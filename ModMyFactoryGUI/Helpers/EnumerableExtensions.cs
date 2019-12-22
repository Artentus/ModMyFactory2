using System.Collections.Generic;
using System.Linq;

namespace ModMyFactoryGUI.Helpers
{
    static class EnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T obj) => Enumerable.Repeat(obj, 1);
    }
}
