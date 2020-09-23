//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Utilities;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.Localization;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Update;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class UpdateWindowViewModel : ScreenBase<UpdateWindow>, IWeakSubscriber<EventArgs>
    {
        public TagVersion CurrentVersion => VersionStatistics.AppVersion;

        public TagVersion UpdateVersion { get; }

        public string Changelog { get; }

        public ICommand UpdateCommand { get; }

        public ICommand CancelCommand { get; }

        public DialogResult DialogResult { get; private set; }

        public UpdateWindowViewModel(TagVersion updateVersion, string changelog)
        {
            UpdateVersion = updateVersion;
            Changelog = changelog;
            DialogResult = DialogResult.None;
            WeakSubscriptionManager.Subscribe(App.Current.Locales, nameof(LocaleManager.UICultureChanged), this);

            UpdateCommand = ReactiveCommand.Create(Update);
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        private void Update()
        {
            DialogResult = DialogResult.Ok;
            AttachedView.Close();
        }

        private void Cancel()
        {
            DialogResult = DialogResult.Cancel;
            AttachedView.Close();
        }

        private void UICultureChangedHandler(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(CurrentVersion));
            this.RaisePropertyChanged(nameof(UpdateVersion));
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e) => UICultureChangedHandler(sender, e);
    }
}
