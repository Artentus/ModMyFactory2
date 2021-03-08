//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Markup.Xaml;
using System.Windows.Input;

namespace ModMyFactoryGUI.Controls
{
    internal sealed class LoginDialog : DialogBase
    {
        private string _username, _password;
        private bool _indicateError;
        private ICommand? _acceptCommand, _cancelCommand;

        public static readonly DirectProperty<LoginDialog, string> UsernameProperty
            = LoginFormular.UsernameProperty.AddOwner<LoginDialog>(
                d => d.Username, (d, v) => d.Username = v, string.Empty);

        public static readonly DirectProperty<LoginDialog, string> PasswordProperty
            = LoginFormular.PasswordProperty.AddOwner<LoginDialog>(
                d => d.Password, (d, v) => d.Password = v, string.Empty);

        public static readonly DirectProperty<LoginDialog, bool> IndicateErrorProperty
            = LoginFormular.IndicateErrorProperty.AddOwner<LoginDialog>(
                d => d.IndicateError, (d, v) => d.IndicateError = v, false);

        public static readonly DirectProperty<LoginDialog, ICommand?> AcceptCommandProperty
            = AvaloniaProperty.RegisterDirect<LoginDialog, ICommand?>(
                nameof(AcceptCommand), d => d.AcceptCommand, (d, v) => d.AcceptCommand = v);

        public static readonly DirectProperty<LoginDialog, ICommand?> CancelCommandProperty
            = AvaloniaProperty.RegisterDirect<LoginDialog, ICommand?>(
                nameof(CancelCommand), d => d.CancelCommand, (d, v) => d.CancelCommand = v);

        public string Username
        {
            get => _username;
            set => SetAndRaise(UsernameProperty, ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetAndRaise(PasswordProperty, ref _password, value);
        }

        public bool IndicateError
        {
            get => _indicateError;
            set => SetAndRaise(IndicateErrorProperty, ref _indicateError, value);
        }

        public ICommand? AcceptCommand
        {
            get => _acceptCommand;
            set => SetAndRaise(AcceptCommandProperty, ref _acceptCommand, value);
        }

        public ICommand? CancelCommand
        {
            get => _cancelCommand;
            set => SetAndRaise(CancelCommandProperty, ref _cancelCommand, value);
        }

        public LoginDialog()
        {
            InitializeComponent();

            _username = string.Empty;
            _password = string.Empty;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
