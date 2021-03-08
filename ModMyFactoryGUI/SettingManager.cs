//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    internal sealed class SettingManager
    {
        private static readonly JsonSerializerSettings _settings;
        private readonly string _filePath;
        private readonly IDictionary<string, object?> _table;

        private SettingManager(string filePath, IDictionary<string, object?> table)
            => (_filePath, _table) = (filePath, table);

        static SettingManager()
        {
            _settings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[]
                {
                    new GridLengthConverter(),
                    new WindowRestoreStateConverter()
                }
            };
        }

        public SettingManager(string filePath)
            : this(filePath, new Dictionary<string, object?>())
        { }

        private static bool TryLoad(string filePath, [NotNullWhen(true)] out SettingManager? result)
        {
            result = null;
            
            try
            {
                if (!File.Exists(filePath)) return false;

                var json = File.ReadAllText(filePath);
                var table = JsonConvert.DeserializeObject<IDictionary<string, object?>>(json, _settings);
                result = new SettingManager(filePath, table!);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error loading settings file");
                return false;
            }
        }

        private static bool TryFindConverter(Type tokenType, Type targetType, [NotNullWhen(true)] out IExtendedJsonConverter? converter)
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

        private static T? Cast<T>(object obj)
        {
            // First try to directly cast to the desired type
            if (obj is T result) return result;

            // Then try to find a suitable converter
            if (TryFindConverter(obj.GetType(), typeof(T), out var converter))
                return (T)converter.CreateFromToken(obj);

            // We couldn't cast the object, let Json.NET do the work for us
            // This may not always work as expected, consider creating a converter
            var token = (JToken)obj;
            return token.ToObject<T>();
        }

        public static SettingManager LoadSafe(string filePath)
        {
            if (TryLoad(filePath, out var manager))
            {
                Log.Debug("Settings loaded:\n\t{0}", string.Join("\n\t",
                    manager._table.Select(kvp => string.Format("{0}: {1}", kvp.Key, kvp.Value))));
            }
            else
            {
                manager = new SettingManager(filePath);
                Log.Warning("Unable to load settings from '{0}', using defaults", filePath);
            }

            return manager;
        }

        public void Set(string key, object value)
            => _table[key] = value;

        public object? Get(string key, object? defaultValue = default)
        {
            if (!_table.TryGetValue(key, out var value))
            {
                value = defaultValue;
                _table.Add(key, value);
            }
            return value;
        }

        public T? Get<T>(string key, T defaultValue = default)
        {
            var obj = Get(key, (object?)defaultValue);
            if (obj is null) return defaultValue;
            return Cast<T>(obj);
        }

        public bool TryGet(string key, out object? result)
            => _table.TryGetValue(key, out result);

        public bool TryGet<T>(string key, out T? result)
        {
            if (TryGet(key, out object? obj))
            {
                if (obj is null) result = default;
                else result = Cast<T>(obj);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public bool Remove(string key)
            => _table.Remove(key);

        public void Save()
        {
            var json = JsonConvert.SerializeObject(_table, Formatting.Indented, _settings);
            File.WriteAllText(_filePath, json, Encoding.UTF8);
            Log.Debug("Settings saved to '{0}'", _filePath);
        }

        public async Task SaveAsync()
        {
            var json = JsonConvert.SerializeObject(_table, Formatting.Indented, _settings);
            await FileHelper.WriteAllTextAsync(_filePath, json);
            Log.Debug("Settings saved to '{0}'", _filePath);
        }
    }
}
