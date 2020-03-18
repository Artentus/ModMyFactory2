//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using ModMyFactory.Win32;
using ModMyFactoryGUI.Helpers;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.Controls
{
    internal enum MessageKind
    {
        None,
        Information,
        Question,
        Warning,
        Error,
    }

    internal sealed class MessageBox : WindowBase
    {
        private readonly ICommand _buttonCommand;

        private MessageBox(string title, string message, MessageKind messageKind, DialogOptions options)
        {
            InitializeComponent();

            Title = title;

            var messageBlock = this.FindControl<TextBlock>("MessageBlock");
            messageBlock.Text = message;

            SetupIcons(messageKind);

            _buttonCommand = ReactiveCommand.Create<DialogResult>(Close);
            SetupButtons(options);
        }

        public MessageBox()
        {
            InitializeComponent();
        }

        private void SetupIcon(string iconName, MessageKind iconKind, MessageKind messageKind)
        {
            var icon = this.FindControl<Control>(iconName);
            if (icon is null) return;

            icon.IsVisible = (iconKind == messageKind);
        }

        private void SetupIcons(MessageKind messageKind)
        {
            foreach (var kind in EnumExtensions.GetValues<MessageKind>())
            {
                string name = $"{kind.Name()}Icon";
                SetupIcon(name, kind, messageKind);
            }
        }

        private void SetupButton(string buttonName, DialogResult result, DialogOptions options)
        {
            var button = this.FindControl<Button>(buttonName);
            if (button is null) return;

            button.Command = _buttonCommand;
            button.CommandParameter = result;
            button.IsVisible = options.HasFlag((DialogOptions)(int)result);
        }

        private void SetupButtons(DialogOptions options)
        {
            foreach (var result in EnumExtensions.GetValues<DialogResult>())
            {
                string name = $"{result.Name()}Button";
                SetupButton(name, result, options);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            // Need to disable resize buttons manually on Windows since Avalonia doesn't.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var handle = PlatformImpl.Handle.Handle;
                var styles = User32.GetWindowStyles(handle);
                styles = styles.UnsetFlag(WindowStyles.MaximizeBox | WindowStyles.MinimizeBox);
                User32.SetWindowStyles(handle, styles);
            }
        }

        public static Task<DialogResult> Show(
            string title, string message, MessageKind messageKind, DialogOptions options)
        {
            var box = new MessageBox(title, message, messageKind, options);
            return box.ShowDialog<DialogResult>(App.Current.MainWindow);
        }

        public void Close(DialogResult result)
            => Close((object)result);
    }
}
