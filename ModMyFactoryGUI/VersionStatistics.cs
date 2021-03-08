//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Export;
using ModMyFactory.Localization;
using ModMyFactory.ModSettings;
using ModMyFactory.WebApi;
using ModMyFactory.Win32;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Update;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

#if SELFCONTAINED
using System.Runtime.InteropServices;
#endif

namespace ModMyFactoryGUI
{
    internal static class VersionStatistics
    {
        private class AssemblyVersionDictionary : IReadOnlyDictionary<Assembly, TagVersion>
        {
            public TagVersion this[Assembly key]
            {
                get
                {
                    if (TryGetValue(key, out TagVersion? value))
                        return value;
                    else
                        throw new KeyNotFoundException();
                }
            }

            public IEnumerable<Assembly> Keys { get; }

            public IEnumerable<TagVersion> Values => Keys.Select(a => TagVersion.Parse(a.ProductVersion()!));

            public int Count => Keys.Count();

            public AssemblyVersionDictionary(IEnumerable<Assembly> assemblies)
            {
                Keys = assemblies;
            }

            public AssemblyVersionDictionary(params Assembly[] assemblies)
                : this((IEnumerable<Assembly>)assemblies)
            { }

            public bool ContainsKey(Assembly key) => Keys.Contains(key);

            public bool TryGetValue(Assembly key, [NotNullWhen(true)] out TagVersion? value)
            {
                if (key is null)
                    throw new ArgumentNullException(nameof(key));

                if (!ContainsKey(key))
                {
                    value = null;
                    return false;
                }

                value = TagVersion.Parse(key.ProductVersion()!);
                return true;
            }

            public IEnumerator<KeyValuePair<Assembly, TagVersion>> GetEnumerator()
            {
                foreach (var assembly in Keys)
                    yield return new KeyValuePair<Assembly, TagVersion>(assembly, TagVersion.Parse(assembly.ProductVersion()!));
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static AppPlatform AppPlatform { get; }

        public static TagVersion AppVersion { get; }

        public static IReadOnlyDictionary<Assembly, TagVersion> LoadedAssemblyVersions { get; }

        static VersionStatistics()
        {
#if SELFCONTAINED
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AppPlatform = AppPlatform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                AppPlatform = AppPlatform.Linux64;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                AppPlatform = AppPlatform.OsX;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
#else
            AppPlatform = AppPlatform.Universal;
#endif

            AppVersion = TagVersion.Parse(Assembly.GetAssembly(typeof(App))!.ProductVersion()!);
            // Just using some arbitrary types to find the assemblies
            LoadedAssemblyVersions = new AssemblyVersionDictionary(
                Assembly.GetAssembly(typeof(Manager))!,
                Assembly.GetAssembly(typeof(AccurateVersion))!,
                Assembly.GetAssembly(typeof(Package))!,
                Assembly.GetAssembly(typeof(Locale))!,
                Assembly.GetAssembly(typeof(ISetting))!,
                Assembly.GetAssembly(typeof(Authentication))!,
                Assembly.GetAssembly(typeof(Kernel32))!
                );
        }
    }
}
