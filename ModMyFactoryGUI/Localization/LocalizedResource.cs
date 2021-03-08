//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Utilities;
using System;
using System.ComponentModel;

namespace ModMyFactoryGUI.Localization
{
    internal sealed class LocalizedResource : NotifyPropertyChangedBase, IWeakSubscriber<EventArgs>
    {
        public string Key { get; }

        public object Value { get; private set; }

        public LocalizedResource(string key)
        {
            Key = key;
            Value = App.Current.Locales.GetResource(Key);
            WeakSubscriptionManager.Subscribe(App.Current.Locales, nameof(LocaleManager.UICultureChanged), this);
        }

        private void UICultureChangedHandler(object sender, EventArgs e)
        {
            Value = App.Current.Locales.GetResource(Key);
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e) => UICultureChangedHandler(sender, e);
    }
}
