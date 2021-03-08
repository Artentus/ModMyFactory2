//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using CommandLine;
using System;

namespace ModMyFactoryGUI.Helpers
{
    internal static class ParserExtensions
    {
        private const char Quote = '"';

        private static string TrimQuotes(this string s)
        {
            if ((s.Length >= 2) && (s[0] == Quote) && (s[s.Length - 1] == Quote))
                return s.Substring(1, s.Length - 2);

            return s;
        }

        private static string[] SplitArgumentString(string argumentString)
        {
            bool insideQuotes = false;

            var result = argumentString.Split(c =>
            {
                if (c == Quote) insideQuotes = !insideQuotes;
                return !insideQuotes && char.IsWhiteSpace(c);
            }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < result.Length; i++)
                result[i] = result[i].Trim().TrimQuotes();

            return result;
        }

        public static ParserResult<T> ParseArguments<T>(this Parser parser, string argumentString)
        {
            string[] args = SplitArgumentString(argumentString);
            return parser.ParseArguments<T>(args);
        }
    }
}
