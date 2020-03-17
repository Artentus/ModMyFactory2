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

        private bool _factorioLocationIsAppData, _factorioLocationIsBinDir, _factorioLocationIsCustom;
        private bool _modLocationIsAppData, _modLocationIsBinDir, _modLocationIsCustom;
        private string _customFactorioLocation, _customModLocation;

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
                    this.RaisePropertyChanged(nameof(Password));

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

        public bool FactorioLocationIsAppData
        {
            get => _factorioLocationIsAppData;
            set
            {
                this.RaiseAndSetIfChanged(ref _factorioLocationIsAppData, value, nameof(FactorioLocationIsAppData));
                SettingsChanged = true;
            }
        }

        public bool FactorioLocationIsBinDir
        {
            get => _factorioLocationIsBinDir;
            set
            {
                this.RaiseAndSetIfChanged(ref _factorioLocationIsBinDir, value, nameof(FactorioLocationIsBinDir));
                SettingsChanged = true;
            }
        }

        public bool FactorioLocationIsCustom
        {
            get => _factorioLocationIsCustom;
            set
            {
                this.RaiseAndSetIfChanged(ref _factorioLocationIsCustom, value, nameof(FactorioLocationIsCustom));
                SettingsChanged = true;
            }
        }

        public bool ModLocationIsAppData
        {
            get => _modLocationIsAppData;
            set
            {
                this.RaiseAndSetIfChanged(ref _modLocationIsAppData, value, nameof(ModLocationIsAppData));
                SettingsChanged = true;
            }
        }

        public bool ModLocationIsBinDir
        {
            get => _modLocationIsBinDir;
            set
            {
                this.RaiseAndSetIfChanged(ref _modLocationIsBinDir, value, nameof(ModLocationIsBinDir));
                SettingsChanged = true;
            }
        }

        public bool ModLocationIsCustom
        {
            get => _modLocationIsCustom;
            set
            {
                this.RaiseAndSetIfChanged(ref _modLocationIsCustom, value, nameof(ModLocationIsCustom));
                SettingsChanged = true;
            }
        }

        public string CustomFactorioLocation
        {
            get => _customFactorioLocation;
            set
            {
                this.RaiseAndSetIfChanged(ref _customFactorioLocation, value, nameof(CustomFactorioLocation));
                if (FactorioLocationIsCustom) SettingsChanged = true;
            }
        }

        public string CustomModLocation
        {
            get => _customModLocation;
            set
            {
                this.RaiseAndSetIfChanged(ref _customModLocation, value, nameof(CustomModLocation));
                if (ModLocationIsCustom) SettingsChanged = true;
            }
        }

        public ICommand ApplyCommand { get; }

        public ICommand ResetCommand { get; }

        public SettingsViewModel()
        {
            Reset();
            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            ResetCommand = ReactiveCommand.Create(Reset);
        }

        private async ValueTask ApplyCredentialChangesAsync()
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
                catch (AuthenticationFailureException)
                {
                    _username = string.Empty;
                    _password = string.Empty;
                    this.RaisePropertyChanged(nameof(Username));
                    this.RaisePropertyChanged(nameof(Password));
                    CredentialsError = true;
                }
                catch (ApiException ex)
                {
                    _username = App.Current.Settings.Get<Credentials>(SettingName.Credentials).Username;
                    this.RaisePropertyChanged(nameof(Username));
                    _password = string.Empty;
                    this.RaisePropertyChanged(nameof(Password));
                    CredentialsError = false;

                    if (ex is ConnectFailureException || ex is TimeoutException)
                    {
                        // Connection error

                        // ToDo: show error message
                    }
                    else
                    {
                        // Server error

                        // ToDo: show error message
                    }
                }
            }
        }

        private async ValueTask ApplyLocationChangesAsync()
        {
            Location factorioLocation = FactorioLocationIsAppData
                ? Location.AppData
                : FactorioLocationIsBinDir
                    ? Location.BinDir
                    : Location.Custom;

            if (factorioLocation != App.Current.Locations.FactorioLocation)
                await App.Current.Locations.MoveFactorioLocationAsync(factorioLocation, _customFactorioLocation);


            Location modLocation = ModLocationIsAppData
                ? Location.AppData
                : ModLocationIsBinDir
                    ? Location.BinDir
                    : Location.Custom;

            if (modLocation != App.Current.Locations.ModLocation)
                await App.Current.Locations.MoveModLocationAsync(modLocation, _customModLocation);
        }

        private async Task ApplyChangesAsync()
        {
            await ApplyCredentialChangesAsync();
            await ApplyLocationChangesAsync();

            SettingsChanged = false;
            App.Current.Settings.Save();
        }

        private void Reset()
        {
            _username = App.Current.Settings.Get<Credentials>(SettingName.Credentials).Username;
            this.RaisePropertyChanged(nameof(Username));
            _password = string.Empty;
            this.RaisePropertyChanged(nameof(Password));
            _credentialsChanged = false;
            CredentialsError = false;

            switch (App.Current.Locations.FactorioLocation)
            {
                case Location.AppData:
                    FactorioLocationIsAppData = true;
                    break;

                case Location.BinDir:
                    FactorioLocationIsBinDir = true;
                    break;

                case Location.Custom:
                    FactorioLocationIsCustom = true;
                    CustomFactorioLocation = App.Current.Locations.CustomFactorioPath;
                    break;
            }

            switch (App.Current.Locations.ModLocation)
            {
                case Location.AppData:
                    ModLocationIsAppData = true;
                    break;

                case Location.BinDir:
                    ModLocationIsBinDir = true;
                    break;

                case Location.Custom:
                    ModLocationIsCustom = true;
                    CustomFactorioLocation = App.Current.Locations.CustomModPath;
                    break;
            }

            SettingsChanged = false;
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
