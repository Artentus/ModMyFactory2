//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.Game;
using ModMyFactoryGUI.Helpers;
using System.Collections.Generic;

namespace ModMyFactoryGUI
{
    internal static class FactorioInstanceExtensions
    {
        private static readonly Dictionary<string, string> NameTable;

        static FactorioInstanceExtensions()
        {
            if (!Program.Settings.TryGet<Dictionary<string, string>>(SettingName.FactorioNameTable, out var nameTable) || (nameTable is null))
                nameTable = new Dictionary<string, string>();
            NameTable = nameTable;
        }

        public static bool IsExternal(this IFactorioInstance instance)
        {
            var instDir = instance.Directory.Parent;
            if (instDir is null) return true;
            var managedDir = Program.Locations.GetFactorioDir();
            return !FileHelper.DirectoriesEqual(instDir, managedDir);
        }

        public static string GetUniqueKey(this IFactorioInstance instance)
        {
            if (instance.IsExternal())
            {
                // We use the full path of the instance as unique key
                string result = instance.Directory.FullName.Trim().ToLower();

                // We have to sanitize the path to make sure it's a proper unique key
                result = result.Replace('/', '_');
                result = result.Replace('\\', '_');
                if (result.EndsWith("_")) result = result[0..^1];

                return result;
            }
            else
            {
                // The directory name is already a unique key, no need to use the full path
                return instance.Directory.Name;
            }
        }

        public static string GetName(this IFactorioInstance instance)
        {
            if (instance.IsSteamInstance())
            {
                // Steam instance has fixed name
                return "Steam";
            }
            else
            {
                string key = instance.GetUniqueKey();
                return NameTable.GetValueOrDefault(key, "Factorio");
            }
        }

        public static void SetName(this IFactorioInstance instance, string name)
        {
            if (instance.IsSteamInstance())
            {
                // Steam instance has fixed name
                return;
            }
            else
            {
                string key = instance.GetUniqueKey();
                NameTable[key] = name;
            }
        }

        public static void SaveNames()
        {
            Program.Settings.Set(SettingName.FactorioNameTable, NameTable);
            Program.Settings.Save();
        }
    }
}
