//  Copyright (C) 2020-2021 Mathis Rech
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
