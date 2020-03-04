//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class SettingsViewModel : MainViewModelBase<SettingsView>
    {
        private bool _settingsChanged;

        private string _username;
        private string _password;
        private bool _credentialsChanged;
        private bool _credentialsError;

        public bool SettingsChanged
        {
            get => _settingsChanged;
            set => this.RaiseAndSetIfChanged(ref _settingsChanged, value, nameof(SettingsChanged));
        }

        public string Username
        {
            get => _username;
            set
            {
                if (value != _username)
                {
                    _username = value;
                    this.RaisePropertyChanged(nameof(Username));

                    _credentialsChanged = true;
                    SettingsChanged = true;
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (value != _password)
                {
                    _password = value;
                    this.RaisePropertyChanging(nameof(Password));

                    _credentialsChanged = true;
                    SettingsChanged = true;
                }
            }
        }

        public bool CredentialsError
        {
            get => _credentialsError;
            set => this.RaiseAndSetIfChanged(ref _credentialsError, value, nameof(CredentialsError));
        }

        public ICommand ApplyCommand { get; }

        public ICommand ResetCommand { get; }

        public SettingsViewModel()
        {
            Reset();
            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            ResetCommand = ReactiveCommand.Create(Reset);
        }

        private async Task ApplyCredentialChangesAsync()
        {
            if (_credentialsChanged)
            {
                _credentialsChanged = false;

                try
                {
                    var (actualName, token) = await Authentication.LogInAsync(Username, Password);

                    // Don't call the property directly since the name hasn't really changed
                    _username = actualName;
                    this.RaisePropertyChanged(nameof(Username));
                    CredentialsError = false;

                    App.Current.Settings.Set(SettingName.Credentials, new Credentials(actualName, token));
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    if (ex.Response is HttpWebResponse response)
                    {
                        if ((response.StatusCode == HttpStatusCode.Unauthorized)
                            || (response.StatusCode == HttpStatusCode.Forbidden))
                        {
                            _username = string.Empty;
                            _password = string.Empty;
                            this.RaisePropertyChanged(nameof(Username));
                            this.RaisePropertyChanged(nameof(Password));
                            CredentialsError = true;
                        }
                        else if ((response.StatusCode == HttpStatusCode.InternalServerError)
                            || (response.StatusCode == HttpStatusCode.Conflict))
                        {
                            // Server error
                            _username = App.Current.Settings.Get<Credentials>(SettingName.Credentials).Username;
                            this.RaisePropertyChanged(nameof(Username));
                            _password = string.Empty;
                            this.RaisePropertyChanged(nameof(Password));
                            CredentialsError = false;

                            // ToDo: show error message
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (WebException ex)
                when ((ex.Status == WebExceptionStatus.ConnectFailure)
                || (ex.Status == WebExceptionStatus.Timeout))
                {
                    // Connection error
                    _username = App.Current.Settings.Get<Credentials>(SettingName.Credentials).Username;
                    this.RaisePropertyChanged(nameof(Username));
                    _password = string.Empty;
                    this.RaisePropertyChanged(nameof(Password));
                    CredentialsError = false;

                    // ToDo: show error message
                }
            }
        }

        private async Task ApplyChangesAsync()
        {
            await ApplyCredentialChangesAsync();

            SettingsChanged = false;
            App.Current.Settings.Save();
        }

        private void Reset()
        {
            SettingsChanged = false;

            _username = App.Current.Settings.Get<Credentials>(SettingName.Credentials).Username;
            this.RaisePropertyChanged(nameof(Username));
            _password = string.Empty;
            this.RaisePropertyChanged(nameof(Password));
            _credentialsChanged = false;
            CredentialsError = false;
        }

        protected override List<IMenuItemViewModel> GetEditMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }

        protected override List<IMenuItemViewModel> GetFileMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }
    }
}
