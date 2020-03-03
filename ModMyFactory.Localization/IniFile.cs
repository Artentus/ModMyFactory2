//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModMyFactory.Localization
{
    /// <summary>
    /// Represents a file in the INI format.
    /// </summary>
    public sealed class IniFile
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data;

        /// <summary>
        /// The empty section/no section.
        /// </summary>
        public const string EmptySection = null;

        private IniFile(Dictionary<string, Dictionary<string, string>> data)
        {
            _data = data;
        }

        public IniFile()
        {
            _data = new Dictionary<string, Dictionary<string, string>>();
        }

        private static Dictionary<string, string> GetSectionDict(Dictionary<string, Dictionary<string, string>> data, string header)
        {
            if (!data.TryGetValue(header, out var sectionDict))
            {
                sectionDict = new Dictionary<string, string>();
                data.Add(header, sectionDict);
            }

            return sectionDict;
        }

        private static bool TryParseLine(string line, ref string currentHeader, Dictionary<string, Dictionary<string, string>> data)
        {
            line = line.TrimStart();

            if (string.IsNullOrEmpty(line)) return true; // empty line
            if (line[0] == ';') return true; // comment line

            if (line[0] == '[') // section header
            {
                if (line[line.Length - 1] == ']') currentHeader = line.Substring(1, line.Length - 2).ToLowerInvariant();
                else return false;
            }
            else
            {
                var parts = line.Split('=');
                if (parts.Length != 2) return false;

                string key = parts[0].TrimEnd().ToLowerInvariant();
                string value = parts[1];

                var sectionDict = GetSectionDict(data, currentHeader);
                sectionDict[key] = value;
            }

            return true;
        }

        private static bool TryParseData(Stream stream, out Dictionary<string, Dictionary<string, string>> result)
        {
            result = new Dictionary<string, Dictionary<string, string>>();

            using (var reader = new StreamReader(stream))
            {
                string line;
                string currentHeader = EmptySection;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!TryParseLine(line, ref currentHeader, result)) return false;
                }
            }

            return true;
        }

        private static bool TryParseData(string iniString, out Dictionary<string, Dictionary<string, string>> result)
        {
            result = new Dictionary<string, Dictionary<string, string>>();

            var lines = iniString.Split('\n');
            string currentHeader = EmptySection;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!TryParseLine(line, ref currentHeader, result)) return false;
            }

            return true;
        }

        private static Dictionary<string, Dictionary<string, string>> MergeData(IEnumerable<Dictionary<string, Dictionary<string, string>>> dataList)
        {
            var sections = new HashSet<string>();
            foreach (var data in dataList)
            {
                var set = new HashSet<string>(data.Keys);
                sections.UnionWith(set);
            }

            var result = new Dictionary<string, Dictionary<string, string>>();
            foreach (var section in sections)
                result.Add(section, new Dictionary<string, string>());

            foreach (var data in dataList)
            {
                foreach (var kvp in data)
                    result[kvp.Key].UnionWith(kvp.Value);
            }

            return result;
        }

        private void AppendDict(StringBuilder sb, Dictionary<string, string> dict)
        {
            foreach (var kvp in dict)
                sb.AppendLine($"{kvp.Key}={kvp.Value}");
        }

        /// <summary>
        /// Tries to read a stream as INI data.
        /// </summary>
        public static bool TryParse(Stream stream, out IniFile result)
        {
            if (TryParseData(stream, out var data))
            {
                result = new IniFile(data);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Reads a stream as INI data.
        /// </summary>
        public static IniFile Parse(Stream stream)
        {
            if (!TryParse(stream, out var result))
                throw new InvalidDataException();
            return result;
        }

        /// <summary>
        /// Tries to read a string as INI data.
        /// </summary>
        public static bool TryParse(string iniString, out IniFile result)
        {
            if (TryParseData(iniString, out var data))
            {
                result = new IniFile(data);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Reads a string as INI data.
        /// </summary>
        public static IniFile Parse(string iniString)
        {
            if (!TryParse(iniString, out var result))
                throw new FormatException();
            return result;
        }

        /// <summary>
        /// Loads an INI file.
        /// </summary>
        public static IniFile Load(FileInfo file)
        {
            using var stream = file.OpenRead();
            return Parse(stream);
        }

        /// <summary>
        /// Loads an INI file.
        /// </summary>
        public static IniFile Load(string fileName)
        {
            var file = new FileInfo(fileName);
            return Load(file);
        }

        /// <summary>
        /// Merges multiple INI files into one.
        /// </summary>
        public static IniFile Merge(IEnumerable<IniFile> iniFiles)
        {
            var mergedDict = MergeData(iniFiles.Select(iniFile => iniFile._data));
            return new IniFile(mergedDict);
        }

        /// <summary>
        /// Merges multiple INI files into one.
        /// </summary>
        public static IniFile Merge(params IniFile[] iniFiles) => Merge((IEnumerable<IniFile>)iniFiles);

        /// <summary>
        /// Tries to get a value in the INI file.
        /// </summary>
        /// <param name="section">The section the value is in. Case insensitive.</param>
        /// <param name="key">The key of the value. Case insensitive.</param>
        public bool TryGetValue(string section, string key, out string result)
        {
            result = null;
            if (!_data.TryGetValue(section.ToLowerInvariant(), out var sectionDict)) return false;
            return sectionDict.TryGetValue(key.ToLowerInvariant(), out result);
        }

        /// <summary>
        /// Gets a value in the INI file.
        /// </summary>
        /// <param name="section">The section the value is in. Case insensitive.</param>
        /// <param name="key">The key of the value. Case insensitive.</param>
        public string GetValue(string section, string key)
        {
            if (!TryGetValue(section, key, out var result))
                throw new KeyNotFoundException();
            return result;
        }

        /// <summary>
        /// Checks if the INI file contains a value in the specified section with the specified key.
        /// </summary>
        /// <param name="section">The section to check in. Case insensitive.</param>
        /// <param name="key">The key to check for. Case insensitive.</param>
        public bool ContainsKey(string section, string key)
        {
            if (!_data.TryGetValue(section.ToLowerInvariant(), out var sectionDict)) return false;
            return sectionDict.ContainsKey(key.ToLowerInvariant());
        }

        /// <summary>
        /// Sets a value in the INI file.
        /// </summary>
        /// <param name="section">The section to set the the value in. Case insensitive.</param>
        /// <param name="key">The key of the value. Case insensitive.</param>
        /// <param name="value">The value to set.</param>
        public void SetValue(string section, string key, string value)
        {
            var sectionDict = GetSectionDict(_data, section.ToLowerInvariant());
            sectionDict[key.ToLowerInvariant()] = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (_data.TryGetValue(EmptySection, out var emptySectionDict))
                AppendDict(sb, emptySectionDict);

            foreach (var kvp in _data)
            {
                if (kvp.Key != EmptySection)
                {
                    sb.AppendLine($"[{kvp.Key}]");
                    AppendDict(sb, kvp.Value);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Saves the INI data to a file.
        /// </summary>
        public void Save(FileInfo file)
        {
            if (!file.Directory.Exists) file.Directory.Create();
            File.WriteAllText(file.FullName, ToString());
        }

        /// <summary>
        /// Saves the INI data to a file.
        /// </summary>
        public void Save(string fileName)
        {
            var file = new FileInfo(fileName);
            Save(file);
        }
    }
}
