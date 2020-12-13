//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Utilities;
using ModMyFactoryGUI.Localization;
using ReactiveUI;
using System;
using System.Reflection;

namespace ModMyFactoryGUI.ViewModels
{
    internal class AssemblyVersionViewModel : ReactiveObject, IWeakSubscriber<EventArgs>
    {
        public string AssemblyName { get; }

        public string AssemblyVersion { get; }

        public AssemblyVersionViewModel(Assembly assembly, string version)
        {
            AssemblyName = assembly.GetName()!.Name!;
            AssemblyVersion = version;
            WeakSubscriptionManager.Subscribe(App.Current.Locales, nameof(LocaleManager.UICultureChanged), this);
        }

        private void UICultureChangedHandler(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(AssemblyVersion));
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e) => UICultureChangedHandler(sender, e);
    }
}
