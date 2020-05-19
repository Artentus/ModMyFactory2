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
using ModMyFactoryGUI.Controls.Icons;
using ModMyFactoryGUI.Localization;
using ModMyFactoryGUI.Localization.Json;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.ViewModels;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    partial class App : Application
    {
        public static event EventHandler Loaded;

        public static event EventHandler ShuttingDown;

        public IClassicDesktopStyleApplicationLifetime Lifetime { get; private set; }

        public MenuItemViewModel ShutdownItemViewModel { get; }

        public LayoutSettings LayoutSettings { get; }

        public LocaleManager Locales { get; }

        public IThemeSelector Themes { get; }

        public CredentialsManager Credentials { get; }

        public static new App Current => Application.Current as App;

        public MainWindow MainWindow => Lifetime.MainWindow as MainWindow;

        public App()
        {
            // GUI-specific global singletons
            LayoutSettings = new LayoutSettings(Program.Settings);
            Locales = LoadLocales();
            Themes = LoadThemes();
            Credentials = new CredentialsManager(Program.Settings);

            Program.Settings.Save();

            // Global shutdown command
            var shutdownCommand = ReactiveCommand.CreateFromTask(ShutdownAsync);
            ShutdownItemViewModel = new MenuItemViewModel(shutdownCommand, null, false, () => new CloseIcon(), "CloseMenuItem", "CloseHotkey");
        }

        private static LocaleManager LoadLocales()
        {
            LocaleManager locales;

            var langDir = new DirectoryInfo(Path.Combine(Program.ApplicationDirectory.FullName, "lang"));
            if (langDir.Exists)
            {
                var localeProvider = new JsonLocaleProvider(langDir);
                try
                {
                    locales = new LocaleManager(localeProvider);
                    Log.Information("Language files successfully loaded. Available languages: {0}",
                        string.Join(", ", locales.AvailableCultures.Select(c => c.EnglishName)));
                }
                catch (LocaleException ex)
                {
                    locales = new LocaleManager();
                    Log.Warning(ex, "Language files could not be loaded");
                }
            }
            else
            {
                locales = new LocaleManager();
                Log.Warning("Language files not found");
            }

            var cultureName = Program.Settings.Get(SettingName.UICulture, LocaleProvider.DefaultCulture);
            var selectedCulture = locales.AvailableCultures.Where(c => string.Equals(c.TwoLetterISOLanguageName, cultureName)).FirstOrDefault();
            if (selectedCulture is null)
            {
                Log.Warning("Language '{0}' not available, reverting to default", cultureName);
                selectedCulture = locales.DefaultCulture;
                Program.Settings.Set(SettingName.UICulture, selectedCulture.TwoLetterISOLanguageName);
            }
            else
            {
                Log.Information("Language set to {0}", selectedCulture.EnglishName);
            }
            locales.UICulture = selectedCulture;
            locales.UICultureChanged += (sender, e) =>
            {
                Program.Settings.Set(SettingName.UICulture, locales.UICulture.TwoLetterISOLanguageName);
                Program.Settings.Save();
            };

            return locales;
        }

        private static IThemeSelector LoadThemes()
        {
            var themeDir = Path.Combine(Program.ApplicationDirectory.FullName, "themes");
            var themes = ThemeSelector.LoadSafe(themeDir);
            Log.Information("Themes successfully loaded; available themes: {0}",
                string.Join(", ", themes.Select(t => t.Name)));

            var themeName = Program.Settings.Get(SettingName.Theme, themes.First().Name);
            if (!themes.SelectTheme(themeName))
            {
                Log.Warning("Theme '{0}' not available, reverting to default", themeName);
                themes.SelectedTheme = themes.First();
                Program.Settings.Set(SettingName.Theme, themes.SelectedTheme.Name);
            }
            else
            {
                Log.Information("Theme set to {0}", themeName);
            }

            themes.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ThemeSelector.SelectedTheme))
                {
                    Program.Settings.Set(SettingName.Theme, themes.SelectedTheme.Name);
                    Program.Settings.Save();
                }
            };

            return themes;
        }

        private void Shutdown() => Lifetime.Shutdown((int)ErrorCode.NoError);

        private async Task ShutdownAsync()
        {
            var uiDispatcher = Dispatcher.UIThread;
            if (uiDispatcher.CheckAccess()) Shutdown();
            else await uiDispatcher.InvokeAsync(Shutdown, DispatcherPriority.Send);
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            ShuttingDown?.Invoke(this, e);
        }

        public bool TryGetThemeResource(string key, out object resource)
            => Themes.SelectedTheme.Style.TryGetResource(key, out resource);

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

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                Lifetime = lifetime;

                Loaded?.Invoke(this, EventArgs.Empty);
                lifetime.Exit += OnExit;

                var mainViewModel = new MainWindowViewModel();
                var mainView = View.CreateAndAttach(mainViewModel);
                lifetime.MainWindow = mainView;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
