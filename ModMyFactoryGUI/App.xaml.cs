using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ThemeManager;
using ModMyFactoryGUI.Localization;
using ModMyFactoryGUI.Localization.Json;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.ViewModels;
using ModMyFactoryGUI.Views;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModMyFactoryGUI
{
    partial class App : Application
    {
        public static new App Current => Application.Current as App;

        public IClassicDesktopStyleApplicationLifetime Lifetime => (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime;

        public MainWindow MainWindow => Lifetime.MainWindow as MainWindow;

        public DirectoryInfo ApplicationDirectory { get; }

        public DirectoryInfo ApplicationDataDirectory { get; }

        public SettingManager Settings { get; private set; }

        public LocaleManager LocaleManager { get; private set; }

        public IThemeSelector ThemeManager { get; private set; }

        DirectoryInfo GetApplicationDataDirectory()
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
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void InitLogger()
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

        void LoadSettings()
        {
            var settingsFile = Path.Combine(ApplicationDataDirectory.FullName, "settings.json");
            Settings = SettingManager.LoadSafe(settingsFile);
        }

        void LoadLocales()
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

        void LoadThemes()
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

        void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
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

                var mainViewModel = new MainWindowViewModel();
                var mainView = View.CreateAndAttach(mainViewModel);
                lifetime.MainWindow = mainView;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
