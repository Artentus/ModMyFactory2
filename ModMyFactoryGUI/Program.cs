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
using ModMyFactory.Export;
using ModMyFactory.Mods;
using ModMyFactoryGUI.CommandLine;
using ModMyFactoryGUI.Helpers;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    internal static class Program
    {
        public const int NoError = 0;
        public const int GeneralError = 1;

        private static ObservableDictionary<int, Modpack> _modpacks;

        // These objects are global to the entire app, even before any GUI is loaded
        public static DirectoryInfo ApplicationDirectory { get; private set; }

        public static DirectoryInfo ApplicationDataDirectory { get; private set; }

        public static DirectoryInfo TemporaryDirectory { get; private set; }

        public static SettingManager Settings { get; private set; }

        public static Manager Manager { get; private set; }

        public static LocationManager Locations { get; private set; }

        public static ObservableDictionary<int, Modpack>.ObservableValueCollection Modpacks => _modpacks.Values;

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

        private static async Task<ObservableDictionary<int, Modpack>> LoadModpacksAsync()
        {
            var result = new ObservableDictionary<int, Modpack>();

            string path = Path.Combine(ApplicationDataDirectory.FullName, "modpacks.json");
            var file = new FileInfo(path);
            if (!file.Exists) return result;

            var imported = await Importer.ImportAsync(file, TemporaryDirectory.FullName, false);
            var package = imported.Package; // Disregard extracted files since there aren't any

            // We have to build a lookup table of mods before we can load any modpacks
            var modMappings = new Dictionary<int, Mod>();
            foreach (var modDef in package.Mods)
            {
                // Usually we'd have to take a whole lot of options into account.
                // However since the package we are loading is created very specifically
                // we can skip most of it and read name and version straight away.
                if (Manager.ContainsMod(modDef.Name, modDef.Version, out Mod mod))
                    modMappings.Add(modDef.Uid, mod);
            }

            foreach (var modpackDef in package.Modpacks)
            {
                var modpack = new Modpack();

                // Instead of throwing an error when a mod or modpack is not found we just ignore it.
                // This way people are free to delete mods from within Factorio or even manually without causing a crash.
                foreach (int modId in modpackDef.ModIds)
                {
                    if (modMappings.TryGetValue(modId, out Mod mod))
                        modpack.Add(mod);
                }
                foreach (int modpackId in modpackDef.ModpackIds)
                {
                    if (result.TryGetValue(modpackId, out var subPack))
                        modpack.Add(subPack);
                }

                result.Add(modpackDef.Uid, modpack);
            }

            return result;
        }

        private static async Task SaveModpacksAsync()
        {
            var factory = new ExporterFactory();

            int id = 0;
            var modMappings = new Dictionary<Mod, int>();
            foreach (var modManager in Manager.ModManagers)
            {
                foreach (var mod in modManager)
                {
                    // Assign an ID to every mod (note we do actually want reference equality here)
                    modMappings.Add(mod, id);

                    var modDef = new ModDefinition(id, mod.Name, ExportMode.SpecificVersion, mod.Version);
                    factory.ModDefinitions.Add(modDef);

                    id++;
                }
            }

            var modpackMappings = _modpacks.Swap(); // Safe here since both directions are unique
            foreach (var modpack in Modpacks)
            {
                var modIds = new List<int>();
                var modpackIds = new List<int>();

                foreach (var child in modpack)
                {
                    if (child is Mod mod)
                    {
                        int modId = modMappings[mod];
                        modIds.Add(modId);
                    }
                    else if (child is Modpack subPack)
                    {
                        int packId = modpackMappings[subPack];
                        modpackIds.Add(packId);
                    }
                }

                var packDef = new ModpackDefinition(modpackMappings[modpack], modpack.DisplayName, modIds, modpackIds);
                factory.ModpackDefinitions.Add(packDef);
            }

            var exporter = factory.CreateExporter();
            string path = Path.Combine(ApplicationDataDirectory.FullName, "modpacks.json");
            await exporter.ExportAsync(path);
        }

        private static async Task InitProgramAsync()
        {
            var factory = new GlobalSingletonFactory(ApplicationDirectory, ApplicationDataDirectory);
            Settings = await factory.LoadSettingsAsync();
            (Manager, Locations) = await factory.CreateManagerAsync(Settings);
            _modpacks = await LoadModpacksAsync();
        }

        private static async Task UnloadProgramAsync()
        {
            await Settings.SaveAsync();
            await SaveModpacksAsync();
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

        private static int GetNextModpackId()
        {
            // Inefficient but doesn't matter as we're realistically only rarely creating modpacks
            int id = 0;
            while (_modpacks.ContainsKey(id)) id++;
            return id;
        }

        public static Modpack CreateModpack()
        {
            var id = GetNextModpackId();
            var modpack = new Modpack();
            _modpacks.Add(id, modpack);
            return modpack;
        }
    }
}
