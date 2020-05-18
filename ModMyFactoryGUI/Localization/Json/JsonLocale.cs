//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ModMyFactoryGUI.Localization.Json
{
    internal sealed class JsonLocale : ILocale
    {
        private readonly IDictionary<string, JsonValue> _values;

        public object this[string key] => _values[key];
        public string Culture { get; }
        public IEnumerable<string> Keys => _values.Keys;
        public IEnumerable<object> Values => _values.Values;
        public int Count => _values.Count;

        public JsonLocale(FileInfo file, string culture)
        {
            using var fs = file.OpenRead();
            using var reader = new StreamReader(fs);

            string json = reader.ReadToEnd();
            _values = JsonConvert.DeserializeObject<IDictionary<string, JsonValue>>(json);

            Culture = culture;
        }

        public bool ContainsKey(string key) => _values.ContainsKey(key);

        public bool TryGetValue(string key, out object value)
        {
            if (_values.TryGetValue(key, out var jsonValue))
            {
                if (jsonValue.Type == JsonValueType.Alias)
                {
                    // We resolve aliases recursively
                    // If the alias key does exist but the aliased key doesn't
                    // we return as if the alias key itself didn't exist
                    string aliasKey = (string)jsonValue.Value;
                    return TryGetValue(aliasKey, out value);
                }
                else
                {
                    value = jsonValue.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var kvp in _values)
            {
                // We need to iterate manuelly to properly resolve aliases
                if (TryGetValue(kvp.Key, out var value))
                    yield return new KeyValuePair<string, object>(kvp.Key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
