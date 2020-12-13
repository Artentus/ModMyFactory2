//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.Helpers;
using Newtonsoft.Json;
using ReactiveUI;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    internal sealed class CredentialsManager
    {
        private readonly struct Credentials
        {
            [JsonProperty("username")]
            public string Username { get; }

            [JsonProperty("token")]
            public string Token { get; }

            [JsonConstructor]
            public Credentials(string username, string token)
                => (Username, Token) = (username, token);

            public void Deconstruct(out string username, out string token)
                => (username, token) = (Username, Token);
        }


        private readonly SettingManager _settings;
        private volatile bool _isLoggedIn;
        private string _username, _token;

        public CredentialsManager(SettingManager settings)
        {
            _settings = settings;
            _username = string.Empty;
            _token = string.Empty;

            if (_settings.TryGet<Credentials>(SettingName.Credentials, out var credentials))
            {
                _isLoggedIn = true;
                (_username, _token) = credentials;
            }
            else
            {
                _isLoggedIn = false;
            }
        }

        private async Task AcceptDialog(LoginDialog dialog)
        {
            var (success, _, _) = await TryLogInAsync(dialog.Username, dialog.Password);
            if (success.HasValue)
            {
                if (success.Value)
                {
                    dialog.Close(DialogResult.Ok);
                }
                else
                {
                    dialog.IndicateError = true;
                }
            }
            else
            {
                dialog.Close(DialogResult.Abort);
            }
        }

        private async Task<(bool? success, string username, string token)> TryLogInWithDialogAsync()
        {
            var dialog = new LoginDialog();
            dialog.CancelCommand = ReactiveCommand.Create(() => dialog.Close(DialogResult.Cancel));
            dialog.AcceptCommand = ReactiveCommand.CreateFromTask(async () => await AcceptDialog(dialog));

            var result = await dialog.ShowDialog<DialogResult>(App.Current.MainWindow);
            return result switch
            {
                DialogResult.Ok => (true, _username, _token),
                DialogResult.Abort => (null, _username, _token),
                _ => (false, null, null)
            };
        }

        public bool TryGetCredentials(out string username, out string token)
        {
            (username, token) = (_username, _token);
            return _isLoggedIn;
        }

        public async ValueTask<(bool? success, string username, string token)> TryLogInAsync()
        {
            if (_isLoggedIn) return (true, _username, _token);
            else return await TryLogInWithDialogAsync();
        }

        public async Task<(bool? success, string? actualName, string? token)> TryLogInAsync(string username, string password)
        {
            try
            {
                (_username, _token) = await Authentication.LogInAsync(username, password);

                // Logged in successfully
                _isLoggedIn = true;
                _settings.Set(SettingName.Credentials, new Credentials(_username, _token));
                _settings.Save();
                return (true, _username, _token);
            }
            catch (AuthenticationFailureException)
            {
                // Login failed
                _isLoggedIn = false;
                _settings.Remove(SettingName.Credentials);
                _settings.Save();
                return (false, null, null);
            }
            catch (ApiException ex)
            {
                // Error occurred when trying to authenticate
                // Display error message and don't change the current logged in state

                await MessageHelper.ShowMessageForApiException(ex);

                // Neither success nor failure
                return (null, _username, _token);
            }
        }
    }
}
