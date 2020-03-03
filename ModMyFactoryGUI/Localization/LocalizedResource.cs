//  Copyright (C) 2020 Mathis Rech
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
    sealed class LocalizedResource : NotifyPropertyChangedBase, IWeakSubscriber<EventArgs>
    {
        public string Key { get; }

        public object Value { get; private set; }

        public LocalizedResource(string key)
        {
            Key = key;
            Value = App.Current.LocaleManager.GetResource(Key);
            WeakSubscriptionManager.Subscribe(App.Current.LocaleManager, nameof(LocaleManager.UICultureChanged), this);
        }

        void UICultureChangedHandler(object sender, EventArgs e)
        {
            Value = App.Current.LocaleManager.GetResource(Key);
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e) => UICultureChangedHandler(sender, e);
    }
}
