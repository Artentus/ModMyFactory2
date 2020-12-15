//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.Tasks.Web;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class SettingsViewModel : MainViewModelBase<SettingsView>
    {
        private readonly DownloadQueue _downloadQueue;

        private bool _settingsChanged;

        private string _username;
        private string _password;
        private bool _credentialsChanged;
        private bool _credentialsError;

        private bool _updateOnStartup, _updatePrerelease;
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

        public bool UpdateOnStartup
        {
            get => _updateOnStartup;
            set
            {
                this.RaiseAndSetIfChanged(ref _updateOnStartup, value, nameof(UpdateOnStartup));
                SettingsChanged = true;
            }
        }

        public bool UpdatePrerelease
        {
            get => _updatePrerelease;
            set
            {
                this.RaiseAndSetIfChanged(ref _updatePrerelease, value, nameof(UpdatePrerelease));
                SettingsChanged = true;
            }
        }

        public bool IsPrerelease => VersionStatistics.AppVersion.IsPrerelease;

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

        public SettingsViewModel(int tabIndex, DownloadQueue downloadQueue)
            : base(tabIndex)
        {
            _downloadQueue = downloadQueue;
            _username = string.Empty;
            _password = string.Empty;
            _customFactorioLocation = string.Empty;
            _customModLocation = string.Empty;

            Reset();

            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            ResetCommand = ReactiveCommand.Create(Reset);
        }

        private async ValueTask ApplyCredentialChangesAsync()
        {
            if (_credentialsChanged)
            {
                _credentialsChanged = false;

                var (success, actualName, _) = await App.Current.Credentials.TryLogInAsync(Username, Password);
                if (success.HasValue)
                {
                    if (success.Value)
                    {
                        // Success
                        // Don't call the property directly since the name hasn't really changed
                        _username = actualName!;
                        this.RaisePropertyChanged(nameof(Username));
                        CredentialsError = false;
                    }
                    else
                    {
                        // Invalid credentials
                        _username = string.Empty;
                        _password = string.Empty;
                        this.RaisePropertyChanged(nameof(Username));
                        this.RaisePropertyChanged(nameof(Password));
                        CredentialsError = true;
                    }
                }
                else
                {
                    // Error
                    _username = actualName!;
                    _password = string.Empty;
                    this.RaisePropertyChanged(nameof(Username));
                    this.RaisePropertyChanged(nameof(Password));
                    CredentialsError = false;
                }
            }
        }

        private async ValueTask ApplyLocationChangesAsync()
        {
            if (_downloadQueue.IsJobInProgress)
            {
                // We don't allow changing locations while downloads are in progress
                // or the app will most definitely crash or corrupt the manager state.
                // For simplicity we don't differentiate between Factorio instances and mods.

                await Messages.MovingLocationsWhileDownloading.Show();
            }
            else
            {
                Location factorioLocation = FactorioLocationIsAppData
                    ? Location.AppData
                    : FactorioLocationIsBinDir
                        ? Location.BinDir
                        : Location.Custom;

                if (factorioLocation != Program.Locations.FactorioLocation)
                    await Program.Locations.MoveFactorioLocationAsync(factorioLocation, _customFactorioLocation);


                Location modLocation = ModLocationIsAppData
                    ? Location.AppData
                    : ModLocationIsBinDir
                        ? Location.BinDir
                        : Location.Custom;

                if (modLocation != Program.Locations.ModLocation)
                    await Program.Locations.MoveModLocationAsync(modLocation, _customModLocation);
            }
        }

        private async Task ApplyChangesAsync()
        {
            await ApplyCredentialChangesAsync();
            await ApplyLocationChangesAsync();

            Program.Settings.Set(SettingName.UpdateOnStartup, _updateOnStartup);
            Program.Settings.Set(SettingName.UpdatePrerelease, _updatePrerelease);

            SettingsChanged = false;
            Program.Settings.Save();
            Reset(); // Reset because some of the changes may not have taken effect
        }

        private void Reset()
        {
            if (!App.Current.Credentials.TryGetCredentials(out _username, out _))
                _username = string.Empty;
            this.RaisePropertyChanged(nameof(Username));
            _password = string.Empty;
            this.RaisePropertyChanged(nameof(Password));
            _credentialsChanged = false;
            CredentialsError = false;

            _updateOnStartup = Program.Settings.Get(SettingName.UpdateOnStartup, true);
            this.RaisePropertyChanged(nameof(UpdateOnStartup));
            if (VersionStatistics.AppVersion.IsPrerelease) _updatePrerelease = true;
            else _updatePrerelease = Program.Settings.Get(SettingName.UpdatePrerelease, false);
            this.RaisePropertyChanged(nameof(UpdatePrerelease));

            switch (Program.Locations.FactorioLocation)
            {
                case Location.AppData:
                    FactorioLocationIsAppData = true;
                    break;

                case Location.BinDir:
                    FactorioLocationIsBinDir = true;
                    break;

                case Location.Custom:
                    FactorioLocationIsCustom = true;
                    CustomFactorioLocation = Program.Locations.CustomFactorioPath!;
                    break;
            }

            switch (Program.Locations.ModLocation)
            {
                case Location.AppData:
                    ModLocationIsAppData = true;
                    break;

                case Location.BinDir:
                    ModLocationIsBinDir = true;
                    break;

                case Location.Custom:
                    ModLocationIsCustom = true;
                    CustomFactorioLocation = Program.Locations.CustomModPath!;
                    break;
            }

            SettingsChanged = false;
        }
    }
}
