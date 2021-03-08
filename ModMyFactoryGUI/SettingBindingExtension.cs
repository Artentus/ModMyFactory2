//  Copyright (C) 2020-2021 Mathis Rech
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
using Avalonia.Styling;
using ModMyFactoryGUI.Helpers;
using System;
using System.ComponentModel;

namespace ModMyFactoryGUI
{
    internal class SettingBindingExtension
    {
        public IValueConverter? Converter { get; set; }

        public object? ConverterParameter { get; set; }

        public BindingMode Mode { get; set; }

        public BindingPriority Priority { get; set; } = BindingPriority.LocalValue;

        public string? StringFormat { get; set; }

        public object TargetNullValue { get; set; } = AvaloniaProperty.UnsetValue;

        [ConstructorArgument("key")]
        public string Key { get; }

        public SettingBindingExtension(string key)
            => Key = key;

        private static object? GetDefaultAnchor(in IServiceProvider context)
        {
            object? anchor = context.GetFirstParent<IControl>();
            anchor ??= context.GetFirstParent<IDataContextProvider>();

            return anchor
                ?? context.GetService<IRootObjectProvider>()?.RootObject as IStyle
                ?? context.GetFirstParent<IStyle>();
        }

        public Binding ProvideValue(IServiceProvider serviceProvider)
        {
            var descriptorContext = (ITypeDescriptorContext)serviceProvider;

            return new Binding
            {
                Converter = Converter,
                ConverterParameter = ConverterParameter,
                Mode = Mode,
                Path = Key,
                Priority = Priority,
                Source = App.Current.LayoutSettings,
                StringFormat = StringFormat,
                DefaultAnchor = new WeakReference(GetDefaultAnchor(descriptorContext)),
                NameScope = new WeakReference<INameScope>(serviceProvider.GetService<INameScope>()!)
            };
        }
    }
}
