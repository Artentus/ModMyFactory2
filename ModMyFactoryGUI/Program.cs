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
using ModMyFactory.Game;
using ModMyFactory.Mods;
using ModMyFactoryGUI.CommandLine;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Synchronization;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    internal static class Program
    {
        private static ObservableDictionary<int, Modpack> _modpacks;
        private static readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(1, 1);

        // These objects are global to the entire app, even before any GUI is loaded
        public static DirectoryInfo ApplicationDirectory { get; private set; }

        public static DirectoryInfo ApplicationDataDirectory { get; private set; }

        public static DirectoryInfo TemporaryDirectory { get; private set; }

        public static SettingManager Settings { get; private set; }

        public static Manager Manager { get; private set; }

        public static LocationManager Locations { get; private set; }

        public static ModStateManager ModStateManager { get; private set; }

        public static ObservableDictionary<int, Modpack>.ObservableValueCollection Modpacks => _modpacks.Values;

        public static GlobalContext SyncContext { get; private set; }

        #region Utility Functions

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

        private static void SetDirectories(OptionsBase options)
        {
            // Application directory
            var assembly = Assembly.GetEntryAssembly();
            ApplicationDirectory = new DirectoryInfo(Path.GetDirectoryName(assembly.Location));

            // Data directory
            if (string.IsNullOrWhiteSpace(options.AppDataPath))
            {
                ApplicationDataDirectory = GetApplicationDataDirectory();
                if (!ApplicationDataDirectory.Exists) ApplicationDataDirectory.Create();
            }
            else
            {
                try
                {
                    ApplicationDataDirectory = new DirectoryInfo(options.AppDataPath);
                    if (!ApplicationDataDirectory.Exists) ApplicationDataDirectory.Create();
                }
                catch
                {
                    // We play it safe here and fall back to the default since at this point
                    // in the app lifetime we cannot properly handle this error except crashing
                    ApplicationDataDirectory = GetApplicationDataDirectory();
                    if (!ApplicationDataDirectory.Exists) ApplicationDataDirectory.Create();
                }
            }

            // Temporary directory
            TemporaryDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "ModMyFactoryGUI"));
            if (!TemporaryDirectory.Exists) TemporaryDirectory.Create();
        }

        private static void InitLogger(OptionsBase options)
        {
            var eventLevel = options.Verbose ? LogEventLevel.Verbose : LogEventLevel.Information;

            var logFile = Path.Combine(ApplicationDataDirectory.FullName, "logs", "log_.txt");
            var config = new LoggerConfiguration()
                .WriteTo.Console(restrictedToMinimumLevel: eventLevel)
#if DEBUG
                .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Verbose)
#endif
                .MinimumLevel.Verbose();

            if (!options.NoLog) config = config.WriteTo.File(
                logFile, restrictedToMinimumLevel: eventLevel,
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);

            Log.Logger = config.CreateLogger();

            Log.Information("Operating system: {0}", RuntimeInformation.OSDescription);
            Log.Information("Runtime: {0}", RuntimeInformation.FrameworkDescription);

            Log.Information("GUI version: {0}", VersionStatistics.AppVersion);
            foreach (var kvp in VersionStatistics.LoadedAssemblyVersions)
                Log.Information("Using {0} v{1}", kvp.Key.GetName().Name, kvp.Value);

            Log.Verbose($"Application directory: {ApplicationDirectory.FullName}");
            Log.Verbose($"Data directory: {ApplicationDataDirectory.FullName}");
        }

        private static async Task<ObservableDictionary<int, Modpack>> LoadModpacksAsync()
        {
            var result = new ObservableDictionary<int, Modpack>();

            string path = Path.Combine(ApplicationDataDirectory.FullName, "modpacks.json");
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                Log.Verbose("No mopack file found, skipping import");
                return result;
            }

            var imported = await Importer.ImportAsync(file, TemporaryDirectory.FullName, false);
            var package = imported.Package; // Disregard extracted files since there aren't any

            if ((package is null) || (package.Mods is null))
            {
                Log.Verbose("No mopacks to import");
                return result;
            }

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
                var modpack = new Modpack { DisplayName = modpackDef.Name };

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
                Log.Verbose($"Successfully loaded modpack '{modpack.DisplayName}' with ID {modpackDef.Uid}");
            }

            return result;
        }

        private static async Task InitProgramAsync()
        {
            var factory = new GlobalSingletonFactory(ApplicationDirectory, ApplicationDataDirectory);
            Settings = factory.LoadSettings();
            (Manager, Locations, ModStateManager) = await factory.CreateManagerAsync(Settings);
            Locations.ModsReloaded += async (s, e) =>
            {
                _modpacks = await LoadModpacksAsync();
            };
            _modpacks = await LoadModpacksAsync();
        }

        private static void UnloadProgram()
        {
            Log.Information("Saving settings...");
            Settings.Save();
            SaveModpacks();
            Log.Information("Shutting down");
            Log.CloseAndFlush();
        }

        #endregion Utility Functions

        #region Internal Functions

        private static Exporter CreateExporter()
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

            return factory.CreateExporter();
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
            var name = (App.Current?.Locales?.GetResource("DefaultModpackName") as string) ?? "New Modpack";
            var modpack = new Modpack { DisplayName = name };
            _modpacks.Add(id, modpack);
            return modpack;
        }

        public static bool DeleteModpack(Modpack modpack)
            => _modpacks.RemoveValue(modpack);

        public static int GetModpackId(Modpack modpack)
        {
            if (modpack is null) throw new ArgumentNullException(nameof(modpack));

            foreach (var kvp in _modpacks)
            {
                if (kvp.Value == modpack)
                    return kvp.Key;
            }

            return -1;
        }

        public static void SaveModpacks()
        {
            var exporter = CreateExporter();
            string path = Path.Combine(ApplicationDataDirectory.FullName, "modpacks.json");
            exporter.Export(path);
        }

        public static async Task SaveModpacksAsync()
        {
            await _syncSemaphore.WaitAsync();

            try
            {
                var exporter = CreateExporter();
                string path = Path.Combine(ApplicationDataDirectory.FullName, "modpacks.json");
                await exporter.ExportAsync(path);
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        #endregion Internal Functions

        #region Initialization

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

        private static async Task<ErrorCode> StartAppAsync(string[] args, RunOptions options)
        {
            SetDirectories(options);

            try
            {
                using (SyncContext = GlobalContext.Create())
                {
                    if (SyncContext.IsFirst)
                    {
                        // First instance, start like normal

                        InitLogger(options);
                        await InitProgramAsync();

                        SyncContext.BeginListen();

                        try
                        {
                            var code = (ErrorCode)BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
                            if (code != ErrorCode.NoError) Log.Warning("Application returned error code '{0}'", code);
                            else Log.Debug("Application exited gracefully");
                            return code;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Application crashed");
                            return ErrorCode.General;
                        }
                        finally
                        {
                            SyncContext.EndListen();
                            UnloadProgram();
                        }
                    }
                    else
                    {
                        // App already running, pass arguments and exit
                        try
                        {
                            var message = Parser.Default.FormatCommandLine(options);
                            await SyncContext.SendMessageAsync(message);

                            return ErrorCode.NoError;
                        }
                        catch
                        {
                            return ErrorCode.General;
                        }
                    }
                }
            }
            finally
            {
                SyncContext = null;
            }
        }

        private static bool TryGetInstance(StartGameOptions options, out IFactorioInstance instance)
        {
            if (!string.IsNullOrEmpty(options.Uid))
            {
                foreach (var inst in Manager.ManagedInstances)
                {
                    if (inst.GetUniqueKey() == options.Uid)
                    {
                        instance = inst;
                        return true;
                    }
                }

                Log.Error($"Factorio instance with ID {options.Uid} does not exist, aborting");
                instance = null;
                return false;
            }

            if (!string.IsNullOrEmpty(options.Name))
            {
                foreach (var inst in Manager.ManagedInstances)
                {
                    if (inst.GetName() == options.Name)
                    {
                        instance = inst;
                        return true;
                    }
                }

                Log.Error($"Factorio instance with name {options.Name} does not exist, aborting");
                instance = null;
                return false;
            }

            instance = null;
            return false;
        }

        private static bool TryGetModpack(StartGameOptions options, out Modpack modpack)
        {
            if (options.ModpackId.HasValue)
            {
                if (_modpacks.TryGetValue(options.ModpackId.Value, out modpack))
                {
                    return true;
                }
                else
                {
                    Log.Warning($"Modpack with ID {options.ModpackId.Value} does not exist, no modpack enabled");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(options.ModpackName))
            {
                foreach (var pack in Modpacks)
                {
                    if (pack.DisplayName == options.ModpackName)
                    {
                        modpack = pack;
                        return true;
                    }
                }

                Log.Warning($"Modpack with name {options.ModpackName} does not exist, no modpack enabled");
                modpack = null;
                return false;
            }

            modpack = null;
            return false;
        }

        private static async Task<ErrorCode> StartGameAsync(StartGameOptions options)
        {
            SetDirectories(options);
            InitLogger(options);
            await InitProgramAsync();

            var code = ErrorCode.NoError;

            try
            {
                if (TryGetInstance(options, out var instance))
                {
                    foreach (var modManager in Manager.ModManagers)
                    {
                        foreach (var mod in modManager)
                            mod.Enabled = false;
                    }

                    if (TryGetModpack(options, out var modpack))
                    {
                        Log.Information($"Enabling modpack {modpack.DisplayName}");
                        modpack.Enabled = true;
                    }

                    ModStateManager.SaveModList(instance.Version);

                    FileInfo savegameFile = null;
                    if (!string.IsNullOrWhiteSpace(options.SavegameFile))
                    {
                        try
                        {
                            savegameFile = new FileInfo(options.SavegameFile);
                            if (!savegameFile.Exists) savegameFile = null;
                        }
                        catch
                        {
                            savegameFile = null;
                        }
                    }

                    Log.Information($"Starting Factorio instance '{instance}'");
                    instance.Start(Locations.GetModDir(instance.Version), savegameFile, options.CustomArguments?.Replace('\'', '"'));
                }
                else
                {
                    code = ErrorCode.GameStart_InvalidInstance;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unable to start Factorio instance");
                code = ErrorCode.GameStart_General;
            }

            UnloadProgram();
            return code;
        }

        // This has not yet been implemented in the latest stable release of CommandLineParser so we have to paste it into here
        private static Task<TResult> MapResultAsync<T1, T2, TResult>(ParserResult<object> result,
            Func<T1, Task<TResult>> parsedFunc1,
            Func<T2, Task<TResult>> parsedFunc2,
            Func<IEnumerable<Error>, Task<TResult>> notParsedFunc)
        {
            if (result is Parsed<object> parsed)
            {
                if (parsed.Value is T1 t1) return parsedFunc1(t1);
                if (parsed.Value is T2 t2) return parsedFunc2(t2);
                throw new InvalidOperationException();
            }
            return notParsedFunc(((NotParsed<object>)result).Errors);
        }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static async Task<int> Main(string[] args)
        {
            bool hasConsole = ConsoleHelper.TryAttachConsole(out var consoleHandle);

            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.HelpWriter = Console.Out;
            });
            var parsedOptions = parser.ParseArguments<RunOptions, StartGameOptions>(args);

            var code = await MapResultAsync(parsedOptions,
                (RunOptions opts) => StartAppAsync(args, opts),
                (StartGameOptions opts) => StartGameAsync(opts),
                errors => Task.FromResult(ErrorCodeFactory.FromCommandLineErrors(errors)));

            if (hasConsole) ConsoleHelper.FreeConsole(consoleHandle);
            return (int)code;
        }

        #endregion Initialization
    }
}
