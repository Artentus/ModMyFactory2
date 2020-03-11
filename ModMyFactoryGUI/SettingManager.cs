//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModMyFactoryGUI
{
    internal sealed class SettingManager
    {
        private static readonly JsonSerializerSettings _settings;
        private readonly string _filePath;
        private readonly IDictionary<string, object> _table;

        private SettingManager(string filePath, IDictionary<string, object> table)
            => (_filePath, _table) = (filePath, table);

        static SettingManager()
        {
            _settings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[]
                {
                    new GridLengthConverter()
                }
            };
        }

        public SettingManager(string filePath)
            : this(filePath, new Dictionary<string, object>())
        { }

        private static bool TryLoad(string filePath, out SettingManager manager)
        {
            manager = default;
            if (!File.Exists(filePath)) return false;

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            try
            {
                var table = JsonConvert.DeserializeObject<IDictionary<string, object>>(json, _settings);
                manager = new SettingManager(filePath, table);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryFindConverter(Type tokenType, Type targetType, out IExtendedJsonConverter converter)
        {
            foreach (var c in _settings.Converters)
            {
                if (c is IExtendedJsonConverter exC)
                {
                    if (c.CanConvert(targetType) && exC.CanConvertFrom(tokenType))
                    {
                        converter = exC;
                        return true;
                    }
                }
            }

            converter = null;
            return false;
        }

        public static SettingManager LoadSafe(string filePath)
        {
            if (!TryLoad(filePath, out var manager))
            {
                manager = new SettingManager(filePath);
                Log.Warning("Unable to load settings from '{0}', using defaults.", filePath);
            }
            else
            {
                Log.Debug("Settings loaded:\n\t{0}", string.Join("\n\t",
                    manager._table.Select(kvp => string.Format("{0}: {1}", kvp.Key, kvp.Value))));
            }
            return manager;
        }

        public void Set(string key, object value)
            => _table[key] = value;

        public object Get(string key, object defaultValue = default)
        {
            if (!_table.TryGetValue(key, out var value))
            {
                value = defaultValue;
                _table.Add(key, value);
            }
            return value;
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            // First try to directly cast to the desired type
            var obj = Get(key, (object)defaultValue);
            if (obj is T result) return result;

            // Then try to find a suitable converter
            if (TryFindConverter(obj.GetType(), typeof(T), out var converter))
                return (T)converter.CreateFromToken(obj);

            // We couldn't cast the object, let Json.NET do the work for us
            // This may not always work as expected, consider creating a converter
            var token = (JToken)obj;
            return token.ToObject<T>();
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(_table, Formatting.Indented, _settings);
            File.WriteAllText(_filePath, json, Encoding.UTF8);
            Log.Debug("Settings saved to '{0}'.", _filePath);
        }
    }
}
