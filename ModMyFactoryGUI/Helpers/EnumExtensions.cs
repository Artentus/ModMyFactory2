using System;

namespace ModMyFactoryGUI.Helpers
{
    static class EnumExtensions
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
    }
}
