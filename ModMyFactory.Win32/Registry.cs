//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Microsoft.Win32;
using System;
using System.IO;

namespace ModMyFactory.Win32
{
    public static class Registry
    {
        private static string? GetString(this RegistryKey key, string? name)
            => key.GetValue(name) as string;

        private static void SetString(this RegistryKey key, string? name, string value)
            => key.SetValue(name, value, RegistryValueKind.String);

        private static bool ContainsName(this RegistryKey key, string name)
            => key.GetValue(name) is not null;

        private static void AddName(this RegistryKey key, string name)
            => key.SetValue(name, Array.Empty<byte>(), RegistryValueKind.None);

        public static string RegisterHandler(string appName, string component, int version, string appPath, string description, string iconPath)
        {
            if (string.IsNullOrEmpty(component)) throw new ArgumentNullException(nameof(component));

            bool changed = false;

            string progId = (version < 0) ? string.Join(".", appName, component) : string.Join(".", appName, component, version);
            RegistryKey? handlerKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Path.Combine(@"Software\Classes", progId));

            if (handlerKey is not null)
            {
                if (!string.IsNullOrEmpty(description))
                {
                    if (!string.Equals(handlerKey.GetString(null), description, StringComparison.InvariantCulture))
                    {
                        handlerKey.SetString(null, description);
                        changed = true;
                    }
                    if (!string.Equals(handlerKey.GetString("FriendlyTypeName"), description, StringComparison.InvariantCulture))
                    {
                        handlerKey.SetString("FriendlyTypeName", description);
                        changed = true;
                    }
                }

                if (!string.IsNullOrEmpty(iconPath))
                {
                    RegistryKey? iconKey = handlerKey.CreateSubKey("DefaultIcon");
                    if ((iconKey is not null) && !string.Equals(iconKey.GetString(null), iconPath, StringComparison.InvariantCulture))
                    {
                        iconKey.SetString(null, iconPath);
                        changed = true;
                    }
                }

                string command = $"{appPath} \"%1\"";
                RegistryKey? openKey = handlerKey.CreateSubKey(@"shell\open\command");
                if ((openKey is not null) && !string.Equals(openKey.GetString(null), command, StringComparison.InvariantCulture))
                {
                    openKey.SetValue(null, command);
                    changed = true;
                }
            }

            if (changed) Shell32.ChangeNotify(ChangeNotifyEventId.AssociationChanged, ChangeNotifyFlags.IdList);

            return progId;
        }

        public static void RegisterFileType(string extension, string handlerName, string mimeType, PercievedFileType percievedType)
        {
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException(nameof(extension));
            if (string.IsNullOrEmpty(handlerName)) throw new ArgumentNullException(nameof(handlerName));

            if (!extension.StartsWith(".")) extension = "." + extension;

            bool changed = false;

            RegistryKey? extensionKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Path.Combine(@"Software\Classes", extension));

            if (extensionKey is not null)
            {
                extensionKey.SetString(null, handlerName);

                if (!string.IsNullOrEmpty(mimeType) && !string.Equals(extensionKey.GetString("Content Type"), mimeType, StringComparison.InvariantCulture))
                {
                    extensionKey.SetString("Content Type", mimeType);
                    changed = true;
                }

                if ((percievedType != PercievedFileType.None) && !string.Equals(extensionKey.GetString("PerceivedType"), percievedType.ToString("g"), StringComparison.InvariantCulture))
                {
                    extensionKey.SetString("PerceivedType", percievedType.ToString("g"));
                    changed = true;
                }

                RegistryKey? openWithKey = extensionKey.CreateSubKey("OpenWithProgids");
                if (openWithKey is not null)
                {
                    if (!openWithKey.ContainsName(handlerName))
                    {
                        openWithKey.AddName(handlerName);
                        changed = true;
                    }
                }
            }

            if (changed) Shell32.ChangeNotify(ChangeNotifyEventId.AssociationChanged, ChangeNotifyFlags.IdList);
        }
    }
}
