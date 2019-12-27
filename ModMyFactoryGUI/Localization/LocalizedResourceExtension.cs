using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using Avalonia.Data.Converters;
using Avalonia;
using ModMyFactoryGUI.Helpers;
using Avalonia.Utilities;

namespace ModMyFactoryGUI.Localization
{
    class LocalizedResourceExtension
    {
        readonly LocalizedResource _resource;
        Control _anchor;

        public IValueConverter Converter { get; set; }

        public object ConverterParameter { get; set; }

        public string ElementName { get; set; }

        public object FallbackValue { get; set; } = AvaloniaProperty.UnsetValue;

        public BindingMode Mode { get; set; }

        [ConstructorArgument("key")]
        public string Key => _resource.Key;

        public BindingPriority Priority { get; set; } = BindingPriority.LocalValue;

        public string StringFormat { get; set; }

        public RelativeSource RelativeSource { get; set; }

        public object TargetNullValue { get; set; } = AvaloniaProperty.UnsetValue;

        public LocalizedResourceExtension(string key)
        {
            _resource = new LocalizedResource(key);
        }

        void DisconnectAnchor(object sender, EventArgs e)
        {
            _anchor.Tag = null;
            _anchor = null;
        }

        void ConnectAnchor(ITypeDescriptorContext context)
        {
            // This is a horrible hack to have the control keep the binding alive.
            var window = context.GetFirstParent<Window>();
            _anchor.Tag = this;
            WeakEventHandlerManager.Subscribe<Window, EventArgs, LocalizedResourceExtension>(window, nameof(Window.Closed), DisconnectAnchor);
        }

        public Binding ProvideValue(IServiceProvider serviceProvider)
        {
            var context = (ITypeDescriptorContext)serviceProvider;
            _anchor = context.GetFirstParent<Control>();
            ConnectAnchor(context);
            
            return new Binding
            {
                TypeResolver = context.ResolveType,
                Converter = Converter,
                ConverterParameter = ConverterParameter,
                ElementName = ElementName,
                FallbackValue = FallbackValue,
                Mode = Mode,
                Path = nameof(LocalizedResource.Value),
                Priority = Priority,
                Source = _resource,
                StringFormat = StringFormat,
                RelativeSource = RelativeSource,
                DefaultAnchor = new WeakReference(_anchor),
                NameScope = new WeakReference<INameScope>(serviceProvider.GetService<INameScope>())
            };
        }
    }
}
