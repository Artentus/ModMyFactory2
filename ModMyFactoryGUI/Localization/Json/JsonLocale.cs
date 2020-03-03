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
    sealed class JsonLocale : ILocale
    {
        readonly IDictionary<string, object> _values;

        public string Culture { get; }

        public object this[string key] => _values[key];
        public IEnumerable<string> Keys => _values.Keys;
        public IEnumerable<object> Values => _values.Values;
        public int Count => _values.Count;

        public JsonLocale(FileInfo file, string culture)
        {
            using var fs = file.OpenRead();
            using var reader = new StreamReader(fs);

            string json = reader.ReadToEnd();
            var dict = JsonConvert.DeserializeObject<IDictionary<string, JsonValue>>(json);

            _values = new Dictionary<string, object>();
            foreach (var kvp in dict)
                _values.Add(kvp.Key, kvp.Value.Value);

            Culture = culture;
        }

        public bool ContainsKey(string key) => _values.ContainsKey(key);
        public bool TryGetValue(string key, out object value) => _values.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_values).GetEnumerator();
    }
}
