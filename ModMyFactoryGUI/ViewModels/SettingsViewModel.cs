//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class SettingsViewModel : MainViewModelBase<SettingsView>
    {
        private bool _settingsChanged;

        private string _username;
        private bool _usernameChanged;

        public bool SettingsChanged
        {
            get => _settingsChanged;
            set
            {
                if (value != _settingsChanged)
                {
                    _settingsChanged = value;
                    this.RaisePropertyChanged(nameof(SettingsChanged));
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (value != _username)
                {
                    _username = value;
                    _usernameChanged = true;
                    this.RaisePropertyChanged(nameof(Username));

                    SettingsChanged = true;
                }
            }
        }

        public string Password { get; set; }

        public ICommand ApplyCommand { get; }

        public ICommand ResetCommand { get; }

        public SettingsViewModel()
        {
            Reset();
            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            ResetCommand = ReactiveCommand.Create(Reset);
        }

        private async Task ApplyChangesAsync()
        {
            //if (_usernameChanged)
            //{
            //    var (actualName, token) = await Authentication.LogInAsync(Username, Password);

            //    // Don't call the property directly since the name hasn't really changed
            //    _username = actualName;
            //    _usernameChanged = false;
            //    this.RaisePropertyChanged(nameof(Username));

            //    App.Current.Settings.Set(SettingName.Credentials, new Credentials(actualName, token));
            //}

            SettingsChanged = false;
            App.Current.Settings.Save();
        }

        private void Reset()
        {
            _settingsChanged = false;

            _username = App.Current.Settings.Get<Credentials>(SettingName.Credentials).Username;
            _usernameChanged = false;
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
