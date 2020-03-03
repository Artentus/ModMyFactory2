//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Utilities;
using ModMyFactoryGUI.Helpers;
using System;
using System.ComponentModel;

namespace ModMyFactoryGUI.Localization
{
    internal class LocalizedResourceExtension
    {
        private readonly LocalizedResource _resource;
        private Control _anchor;

        public IValueConverter Converter { get; set; }

        public object ConverterParameter { get; set; }

        public string ElementName { get; set; }

        public object FallbackValue { get; set; } = AvaloniaProperty.UnsetValue;

        public BindingMode Mode { get; set; }

        public BindingPriority Priority { get; set; } = BindingPriority.LocalValue;

        public string StringFormat { get; set; }

        public RelativeSource RelativeSource { get; set; }

        public object TargetNullValue { get; set; } = AvaloniaProperty.UnsetValue;

        [ConstructorArgument("key")]
        public string Key => _resource.Key;

        public LocalizedResourceExtension(string key)
        {
            _resource = new LocalizedResource(key);
        }

        private void DisconnectAnchor(object sender, EventArgs e)
        {
            _anchor.Tag = null;
            _anchor = null;
        }

        private void Subscribe(Window window)
        {
            WeakEventHandlerManager.Subscribe<Window, EventArgs, LocalizedResourceExtension>(window, nameof(Window.Closed), DisconnectAnchor);
        }

        private void DelayedSubscribe(object sender, EventArgs e)
        {
            var userControl = (UserControl)sender;

            IControl control = userControl;
            while (!(control is null))
            {
                if (control is Window) break;
                control = control.Parent;
            }

            try
            {
                if (control is Window window) Subscribe(window);
                else throw new InvalidOperationException("No window found");
            }
            catch
            {
                _anchor.Tag = null;
                _anchor = null;
                throw;
            }
            finally
            {
                userControl.AttachedToLogicalTree -= DelayedSubscribe;
            }
        }

        private void ConnectAnchor(ITypeDescriptorContext context)
        {
            // This is a horrible hack to have the control keep the binding alive.
            _anchor.Tag = this;
            var window = context.GetFirstParent<Window>();
            if (window is null)
            {
                var userControl = context.GetFirstParent<UserControl>();
                userControl.AttachedToLogicalTree += DelayedSubscribe;
            }
            else
            {
                Subscribe(window);
            }
        }

        public Binding ProvideValue(IServiceProvider serviceProvider)
        {
            var context = (ITypeDescriptorContext)serviceProvider;
            _anchor = context.GetFirstParent<Control>();
            if (_anchor is null) throw new InvalidOperationException("No suitable anchor found");
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
