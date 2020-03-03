//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using System;
using System.Linq;

namespace ModMyFactoryGUI.Helpers
{
    // Code directly taken from Avalonia since it's internal.
    static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider sp) => (T)sp?.GetService(typeof(T));

        public static T GetFirstParent<T>(this IServiceProvider ctx) where T : class
            => ctx.GetService<IAvaloniaXamlIlParentStackProvider>().Parents.OfType<T>().FirstOrDefault();

        public static Type ResolveType(this IServiceProvider ctx, string namespacePrefix, string type)
        {
            var tr = ctx.GetService<IXamlTypeResolver>();
            string name = string.IsNullOrEmpty(namespacePrefix) ? type : $"{namespacePrefix}:{type}";
            return tr?.Resolve(name);
        }
    }
}
