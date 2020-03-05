//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactoryGUI.Helpers
{
    internal static class TimeSpanExtensions
    {
        public static TimeSpan MultiplyEx(this TimeSpan timeSpan, double factor)
        {
#if NETCORE
            return timeSpan.Multiply(factor);
#else
            if (double.IsNaN(factor)) throw new ArgumentException("Cannot multiply by Double.NaN", nameof(factor));

            double ticks = Math.Round(timeSpan.Ticks * factor);
            if (ticks > long.MaxValue || ticks < long.MinValue)
                throw new OverflowException();

            return TimeSpan.FromTicks((long)ticks);
#endif
        }

        public static TimeSpan DivideEx(this TimeSpan timeSpan, double divisor)
        {
#if NETCORE
            return timeSpan.Divide(divisor);
#else
            if (double.IsNaN(divisor)) throw new ArgumentException("Cannot divide by Double.NaN", nameof(divisor));

            double ticks = Math.Round(timeSpan.Ticks / divisor);
            if (ticks > long.MaxValue || ticks < long.MinValue || double.IsNaN(ticks))
                throw new OverflowException();

            return TimeSpan.FromTicks((long)ticks);
#endif
        }

        public static TimeSpan Max(this TimeSpan first, TimeSpan second)
        {
            if (second > first) return second;
            return first;
        }

        public static TimeSpan Min(this TimeSpan first, TimeSpan second)
        {
            if (second < first) return second;
            return first;
        }
    }
}
