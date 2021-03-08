//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Media.Imaging;
using Avalonia.ThemeManager;
using Avalonia.Utilities;
using ModMyFactoryGUI.Localization;
using ReactiveUI;
using System;
using System.IO;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ThemeViewModel : ReactiveObject, IWeakSubscriber<EventArgs>
    {
        private class EventManager
        {
            public event EventHandler? SelectedThemeChanged;

            public void RaiseEvent()
                => SelectedThemeChanged?.Invoke(this, EventArgs.Empty);
        }


        private const string ResourcePrefix = "__theme__.";
        private static readonly EventManager InternalEventManager = new EventManager();

        public ITheme Theme { get; }

        public string DisplayName => (string)App.Current.Locales.GetResource(ResourcePrefix + Theme.Name);

        public IBitmap? Icon { get; }

        public ICommand SelectCommand { get; }

        public bool Selected => App.Current.Themes.SelectedTheme == Theme;

        public ThemeViewModel(ITheme theme)
        {
            Theme = theme;

            string iconPath = Path.Combine(Program.ApplicationDirectory.FullName,
                "themes", "assets", "icons", theme.Name + ".png");
            if (File.Exists(iconPath)) Icon = new Bitmap(iconPath);

            SelectCommand = ReactiveCommand.Create(Select);
            WeakSubscriptionManager.Subscribe(InternalEventManager, nameof(EventManager.SelectedThemeChanged), this);
            WeakSubscriptionManager.Subscribe(App.Current.Locales, nameof(LocaleManager.UICultureChanged), this);
        }

        private void SelectedThemeChangedHandler() => this.RaisePropertyChanged(nameof(Selected));

        private void UICultureChangedHandler() => this.RaisePropertyChanged(nameof(DisplayName));

        public static void SubscribeWeak(IWeakSubscriber<EventArgs> subscriber)
            => WeakSubscriptionManager.Subscribe(InternalEventManager, nameof(EventManager.SelectedThemeChanged), subscriber);

        public void Select()
        {
            App.Current.Themes.SelectedTheme = Theme;
            InternalEventManager.RaiseEvent();
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e)
        {
            if (sender is EventManager)
                SelectedThemeChangedHandler();
            else if (sender is LocaleManager)
                UICultureChangedHandler();
        }
    }
}
