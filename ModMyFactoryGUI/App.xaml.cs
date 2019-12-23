using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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

        public LocaleManager LocaleManager { get; private set; }

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

        void InitLogger(IClassicDesktopStyleApplicationLifetime lifetime)
        {
            string logFile = Path.Combine(ApplicationDataDirectory.FullName, "logs", "log_.txt");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File(logFile, restrictedToMinimumLevel: LogEventLevel.Information,
                              rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
#if DEBUG
                .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Verbose)
#endif
                .MinimumLevel.Verbose()
                .CreateLogger();

            Log.Information("GUI version: {0}");
            Log.Information("ModMyFactory version: {0}");

            lifetime.Exit += (sender, e) => Log.CloseAndFlush();
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
                        string.Join(", ", LocaleManager.AvailableCultures.Select(c => c.TwoLetterISOLanguageName)));
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
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                InitLogger(lifetime);
                LoadLocales();
                lifetime.MainWindow = View.CreateWithViewModel<MainWindow, MainWindowViewModel>(out _);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
