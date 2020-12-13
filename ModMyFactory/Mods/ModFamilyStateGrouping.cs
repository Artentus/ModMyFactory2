//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// Groups information about the state of multiple mod families
    /// </summary>
    public sealed class ModFamilyStateGrouping
    {
        [JsonProperty("mods")]
        public IEnumerable<ModFamilyStateInfo> States { get; }

        [JsonConstructor]
        private ModFamilyStateGrouping(IEnumerable<ModFamilyStateInfo> states)
        {
            States = states;
        }

        /// <summary>
        /// Creates and groups state information of all families in a mod manager
        /// </summary>
        public static ModFamilyStateGrouping FromManager(ModManager manager, bool includeBase = false)
        {
            var states = new List<ModFamilyStateInfo>(manager.Families.Count + (includeBase ? 1 : 0));
            if (includeBase) states.Add(ModFamilyStateInfo.Base);
            foreach (var family in manager.Families)
                states.Add(ModFamilyStateInfo.FromFamily(family));
            return new ModFamilyStateGrouping(states);
        }

        /// <summary>
        /// Creates state information from a JSON string
        /// </summary>
        public static ModFamilyStateGrouping FromJson(string json)
            => JsonConvert.DeserializeObject<ModFamilyStateGrouping>(json);

        /// <summary>
        /// Loads state information from a file
        /// </summary>
        public static ModFamilyStateGrouping FromFile(string path)
        {
            string json = File.ReadAllText(path);
            return FromJson(json);
        }

        /// <summary>
        /// Loads state information from a file
        /// </summary>
        public static ModFamilyStateGrouping FromFile(FileInfo file)
            => FromFile(file.FullName);

        /// <summary>
        /// Loads state information from a file
        /// </summary>
        public static async Task<ModFamilyStateGrouping> FromFileAsync(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                                              4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

            var buffer = new byte[(int)stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            string json = Encoding.UTF8.GetString(buffer);

            return FromJson(json);
        }

        /// <summary>
        /// Loads state information from a file
        /// </summary>
        public static async Task<ModFamilyStateGrouping> FromFileAsync(FileInfo file)
            => await FromFileAsync(file.FullName);

        /// <summary>
        /// Creates a JSON string from this state grouping
        /// </summary>
        public string ToJson(Formatting formatting = Formatting.Indented, JsonSerializerSettings? settings = null)
            => JsonConvert.SerializeObject(this, formatting, settings);

        /// <summary>
        /// Saves all state information to a file
        /// </summary>
        public void SaveToFile(string path, Formatting formatting = Formatting.Indented, JsonSerializerSettings? settings = null)
        {
            string json = ToJson(formatting, settings);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Saves all state information to a file
        /// </summary>
        public void SaveToFile(FileInfo file, Formatting formatting = Formatting.Indented, JsonSerializerSettings? settings = null)
            => SaveToFile(file.FullName, formatting, settings);

        /// <summary>
        /// Saves all state information to a file
        /// </summary>
        public async Task SaveToFileAsync(string path, Formatting formatting = Formatting.Indented, JsonSerializerSettings? settings = null)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                                              4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

            string json = ToJson(formatting, settings);
            var buffer = Encoding.UTF8.GetBytes(json);

            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Saves all state information to a file
        /// </summary>
        public async Task SaveToFileAsync(FileInfo file, Formatting formatting = Formatting.Indented, JsonSerializerSettings? settings = null)
        {
            if (!file.Directory.Exists) file.Directory.Create();

            using var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None,
                                              4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

            string json = ToJson(formatting, settings);
            var buffer = Encoding.UTF8.GetBytes(json);

            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Applies state information to a mod manager
        /// </summary>
        public void ApplyToManager(in ModManager manager)
        {
            if (manager is null)
                throw new ArgumentNullException(nameof(manager));

            foreach (var state in States)
            {
                if (manager.Contains(state.FamilyName, out var family))
                    state.ApplyToFamily(family);
            }
        }
    }
}
