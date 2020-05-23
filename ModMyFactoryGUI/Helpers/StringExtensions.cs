//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections.Generic;

namespace ModMyFactoryGUI.Helpers
{
    internal static class StringExtensions
    {
        // String.Split based on a selector function
        public static string[] Split(this string s, Func<char, bool> selector, StringSplitOptions options = StringSplitOptions.None)
        {
            var result = new List<string>();

            int start = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (selector(s[i]))
                {
                    var part = s.Substring(start, i - start);
                    if ((part.Length > 0) || (options != StringSplitOptions.RemoveEmptyEntries)) result.Add(part);
                    start = i + 1;
                }
            }

            if (start < s.Length)
            {
                var last = s.Substring(start);
                if ((last.Length > 0) || (options != StringSplitOptions.RemoveEmptyEntries)) result.Add(last);
            }

            return result.ToArray();
        }
    }
}
