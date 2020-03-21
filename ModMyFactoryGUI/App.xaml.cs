//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ThemeManager;
using Avalonia.Threading;
using ModMyFactory;
using ModMyFactoryGUI.Controls.Icons;
using ModMyFactoryGUI.Localization;
using ModMyFactoryGUI.Localization.Json;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.ViewModels;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    partial class App : Application
    {
        public static new App Current => Application.Current as App;

        public static event EventHandler Loaded;

        public static event EventHandler ShuttingDown;

        public IClassicDesktopStyleApplicationLifetime Lifetime => (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime;

        public MainWindow MainWindow => Lifetime.MainWindow as MainWindow;

        public MenuItemViewModel ShutdownItemViewModel { get; }

        public DirectoryInfo ApplicationDirectory { get; }

        public DirectoryInfo ApplicationDataDirectory { get; }

        public SettingManager Settings { get; private set; }

        public LayoutSettings LayoutSettings { get; private set; }

        public LocaleManager LocaleManager { get; private set; }

        public IThemeSelector ThemeManager { get; private set; }

        public Manager Manager { get; private set; }

        public LocationManager Locations { get; private set; }

        public CredentialsManager Credentials { get; private set; }

        public bool TryGetThemeResource(string key, out object resource)
            => ThemeManager.SelectedTheme.Style.TryGetResource(key, out resource);

        public bool TryGetThemeResource<T>(string key, out T resource)
        {
            if (TryGetThemeResource(key, out object objRes))
            {
                if (objRes is T tRes)
                {
                    resource = tRes;
                    return true;
                }
            }

            resource = default;
            return false;
        }

        private void Shutdown() => Lifetime.Shutdown();

        private async Task ShutdownAsync()
        {
            var uiDispatcher = Dispatcher.UIThread;
            if (uiDispatcher.CheckAccess()) Shutdown();
            else await uiDispatcher.InvokeAsync(Shutdown, DispatcherPriority.Send);
        }

        private DirectoryInfo GetApplicationDataDirectory()
        {
            string path = null;
#if NETFULL
            path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "ModMyFactoryGUI");
#elif NETCORE
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                path = Path.Combine(path, "ModMyFactoryGUI");
            }
            else if (os.Platform == PlatformID.Unix)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                path = Path.Combine(path, ".modmyfactorygui");
            }
#endif
            return new DirectoryInfo(path);
        }

        public App()
        {
            // Application directory
            var assembly = Assembly.GetEntryAssembly();
            ApplicationDirectory = new DirectoryInfo(Path.GetDirectoryName(assembly.Location));

            // Data directory
            ApplicationDataDirectory = GetApplicationDataDirectory();
            if (!ApplicationDataDirectory.Exists) ApplicationDataDirectory.Create();

            // Global shutdown command
            var shutdownCommand = ReactiveCommand.CreateFromTask(ShutdownAsync);
            ShutdownItemViewModel = new MenuItemViewModel(shutdownCommand, false, () => new CloseIcon(), "CloseMenuItem", "CloseHotkey");
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InitLogger()
        {
            var logFile = Path.Combine(ApplicationDataDirectory.FullName, "logs", "log_.txt");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File(logFile, restrictedToMinimumLevel: LogEventLevel.Information,
                              rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
#if DEBUG
                .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Verbose)
#endif
                .MinimumLevel.Verbose()
                .CreateLogger();

            Log.Information("GUI version: {0}", VersionStatistics.AppVersion);
            foreach (var kvp in VersionStatistics.LoadedAssemblyVersions)
                Log.Information("{0} v{1}", kvp.Key.GetName().Name, kvp.Value);
        }

        private void LoadSettings()
        {
            var settingsFile = Path.Combine(ApplicationDataDirectory.FullName, "settings.json");
            Settings = SettingManager.LoadSafe(settingsFile);
            LayoutSettings = new LayoutSettings(Settings);
        }

        private void LoadLocales()
        {
            var langDir = new DirectoryInfo(Path.Combine(ApplicationDirectory.FullName, "lang"));
            if (langDir.Exists)
            {
                var localeProvider = new JsonLocaleProvider(langDir);
                try
                {
                    LocaleManager = new LocaleManager(localeProvider);
                    Log.Information("Language files successfully loaded. Available languages: {0}",
                        string.Join(", ", LocaleManager.AvailableCultures.Select(c => c.EnglishName)));
                }
                catch (LocaleException ex)
                {
                    LocaleManager = new LocaleManager();
                    Log.Warning(ex, "Language files could not be loaded.");
                }
            }
            else
            {
                LocaleManager = new LocaleManager();
                Log.Warning("Language files not found.");
            }

            var cultureName = Settings.Get(SettingName.UICulture, LocaleProvider.DefaultCulture);
            var selectedCulture = LocaleManager.AvailableCultures.Where(c => string.Equals(c.TwoLetterISOLanguageName, cultureName)).FirstOrDefault();
            if (selectedCulture is null)
            {
                Log.Warning("Language '{0}' not available, reverting to default.", cultureName);
                selectedCulture = LocaleManager.DefaultCulture;
                Settings.Set(SettingName.UICulture, selectedCulture.TwoLetterISOLanguageName);
            }
            else
            {
                Log.Information("Language set to {0}.", selectedCulture.EnglishName);
            }
            LocaleManager.UICulture = selectedCulture;
            LocaleManager.UICultureChanged += (sender, e) =>
            {
                Settings.Set(SettingName.UICulture, LocaleManager.UICulture.TwoLetterISOLanguageName);
                Settings.Save();
            };
        }

        private void LoadThemes()
        {
            var themeDir = Path.Combine(ApplicationDirectory.FullName, "themes");
            ThemeManager = ThemeSelector.LoadSafe(themeDir);
            Log.Information("Themes successfully loaded. Available themes: {0}",
                string.Join(", ", ThemeManager.Select(t => t.Name)));

            var themeName = Settings.Get(SettingName.Theme, ThemeManager.First().Name);
            if (!ThemeManager.SelectTheme(themeName))
            {
                Log.Warning("Theme '{0}' not available, reverting to default.", themeName);
                ThemeManager.SelectedTheme = ThemeManager.First();
                Settings.Set(SettingName.Theme, ThemeManager.SelectedTheme.Name);
            }
            else
            {
                Log.Information("Theme set to {0}.", themeName);
            }

            ThemeManager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ThemeSelector.SelectedTheme))
                {
                    Settings.Set(SettingName.Theme, ThemeManager.SelectedTheme.Name);
                    Settings.Save();
                }
            };
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Settings.Save();
            Log.CloseAndFlush();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                InitLogger();
                LoadSettings();
                LoadLocales();
                LoadThemes();

                Settings.Save();
                lifetime.Exit += OnExit;

                Manager = new Manager();
                Locations = new LocationManager(Manager, Settings);
                Credentials = new CredentialsManager(Settings);

                Loaded?.Invoke(this, EventArgs.Empty);
                lifetime.Exit += (sender, e) => ShuttingDown(this, EventArgs.Empty);

                var mainViewModel = new MainWindowViewModel();
                var mainView = View.CreateAndAttach(mainViewModel);
                lifetime.MainWindow = mainView;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
