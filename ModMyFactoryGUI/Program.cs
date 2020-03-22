//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using CommandLine;
using ModMyFactory;
using ModMyFactoryGUI.CommandLine;
using ModMyFactoryGUI.Helpers;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    internal static class Program
    {
        public const int NoError = 0;
        public const int GeneralError = 1;

        // These objects are global to the entire app, even before any GUI is loaded
        public static DirectoryInfo ApplicationDirectory { get; private set; }

        public static DirectoryInfo ApplicationDataDirectory { get; private set; }

        public static DirectoryInfo TemporaryDirectory { get; private set; }

        public static SettingManager Settings { get; private set; }

        public static Manager Manager { get; private set; }

        public static LocationManager Locations { get; private set; }

        private static int StartGame(StartGameOptions options)
        {
            return NoError;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
#if DEBUG
                .LogToDebug()
#endif
                .UsePlatformDetect()
                .UseReactiveUI();
        }

        private static int StartApp(string[] args, OptionsBase options)
        {
            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }

        private static DirectoryInfo GetApplicationDataDirectory()
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

        private static void SetDirectories()
        {
            // Application directory
            var assembly = Assembly.GetEntryAssembly();
            ApplicationDirectory = new DirectoryInfo(Path.GetDirectoryName(assembly.Location));

            // Data directory
            ApplicationDataDirectory = GetApplicationDataDirectory();
            if (!ApplicationDataDirectory.Exists) ApplicationDataDirectory.Create();

            // Temporary directory
            TemporaryDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "ModMyFactoryGUI"));
            if (!TemporaryDirectory.Exists) TemporaryDirectory.Create();
        }

        private static void InitLogger()
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

        private static async Task InitProgramAsync()
        {
            var factory = new GlobalSingletonFactory(ApplicationDirectory, ApplicationDataDirectory);
            Settings = await factory.LoadSettingsAsync();
            (Manager, Locations) = await factory.CreateManagerAsync(Settings);
        }

        private static async Task UnloadProgramAsync()
        {
            await Settings.SaveAsync();
            Log.CloseAndFlush();
        }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static async Task<int> Main(string[] args)
        {
            bool hasConsole = ConsoleHelper.TryAttachConsole(out var consoleHandle);

            SetDirectories();
            InitLogger();
            await InitProgramAsync();

            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.HelpWriter = Console.Out;
            });
            var parsedOptions = parser.ParseArguments<RunOptions, StartGameOptions>(args);

            int result = parsedOptions.MapResult(
                (RunOptions opts) => StartApp(args, opts),
                (StartGameOptions opts) => StartGame(opts),
                errs => GeneralError);

            await UnloadProgramAsync();

            if (hasConsole) ConsoleHelper.FreeConsole(consoleHandle);
            return result;
        }
    }
}
