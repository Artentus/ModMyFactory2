using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModMyFactoryGUI
{
    sealed class SettingManager
    {
        readonly string _filePath;
        readonly IDictionary<string, object> _table;

        private SettingManager(string filePath, IDictionary<string, object> table)
            => (_filePath, _table) = (filePath, table);

        public SettingManager(string filePath)
            : this(filePath, new Dictionary<string, object>())
        { }

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
            => (T)Get(key, (object)defaultValue);

        public void Save()
        {
            var json = JsonConvert.SerializeObject(_table, Formatting.Indented);
            File.WriteAllText(_filePath, json, Encoding.UTF8);
            Log.Debug("Settings saved to '{0}'.", _filePath);
        }

        static bool TryLoad(string filePath, out SettingManager manager)
        {
            manager = default;
            if (!File.Exists(filePath)) return false;

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            try
            {
                var table = JsonConvert.DeserializeObject<IDictionary<string, object>>(json);
                manager = new SettingManager(filePath, table);
                return true;
            }
            catch
            {
                return false;
            }
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
    }
}
