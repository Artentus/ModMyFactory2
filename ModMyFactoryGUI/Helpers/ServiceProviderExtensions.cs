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
