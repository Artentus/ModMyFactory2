//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace ModMyFactoryGUI.Controls
{
    internal class LoginFormular : UserControl
    {
        private string _username, _password;
        private bool _indicateError;

        public static readonly DirectProperty<LoginFormular, string> UsernameProperty
            = AvaloniaProperty.RegisterDirect<LoginFormular, string>(
                nameof(Username), f => f.Username, (f, v) => f.Username = v, string.Empty, BindingMode.TwoWay, true);

        public static readonly DirectProperty<LoginFormular, string> PasswordProperty
            = AvaloniaProperty.RegisterDirect<LoginFormular, string>(
                nameof(Password), f => f.Password, (f, v) => f.Password = v, string.Empty, BindingMode.TwoWay, true);

        public static readonly DirectProperty<LoginFormular, bool> IndicateErrorProperty
            = AvaloniaProperty.RegisterDirect<LoginFormular, bool>(
                nameof(IndicateError), f => f.IndicateError, (f, v) => f.IndicateError = v, false);

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
            set
            {
                SetAndRaise(IndicateErrorProperty, ref _indicateError, value);

                if (value)
                {
                    Username = string.Empty;
                    Password = string.Empty;
                }
            }
        }

        public LoginFormular()
        {
            InitializeComponent();

            PseudoClass<LoginFormular>(IndicateErrorProperty, ":error");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
