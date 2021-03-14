//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommandLine;
using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Game;
using ModMyFactory.WebApi.Factorio;
using ModMyFactoryGUI.CommandLine;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Tasks.Web;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class FactorioInstanceViewModel : ReactiveObject
    {
        private readonly Manager _manager;
        private readonly LocationManager? _locations;
        private bool _isInDownloadQueue, _isDownloading, _isExtracting, _isInstalled, _isRenaming;
        private ManagedFactorioInstance? _instance;

        public event EventHandler? InstanceRemoved;

        public bool IsInDownloadQueue
        {
            get => _isInDownloadQueue;
            private set => this.RaiseAndSetIfChanged(ref _isInDownloadQueue, value, nameof(IsInDownloadQueue));
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            private set => this.RaiseAndSetIfChanged(ref _isDownloading, value, nameof(IsDownloading));
        }

        public bool IsExtracting
        {
            get => _isExtracting;
            private set => this.RaiseAndSetIfChanged(ref _isExtracting, value, nameof(IsExtracting));
        }

        public bool IsInstalled
        {
            get => _isInstalled;
            private set => this.RaiseAndSetIfChanged(ref _isInstalled, value, nameof(IsInstalled));
        }

        public bool IsRenaming
        {
            get => _isRenaming;
            set
            {
                if (value != _isRenaming)
                {
                    _isRenaming = value;
                    if (!_isRenaming) FactorioInstanceExtensions.SaveNames();
                    this.RaisePropertyChanged(nameof(IsRenaming));
                }
            }
        }

        public double DownloadProgress { get; private set; }

        // True if the instance does not reside inside the managed directory
        public bool IsExternal { get; }

        public ManagedFactorioInstance? Instance
        {
            get => _instance;
            private set
            {
                if (value != _instance)
                {
                    _instance = value;
                    this.RaisePropertyChanged(nameof(Instance));

                    this.RaisePropertyChanged(nameof(Name));
                    this.RaisePropertyChanged(nameof(Version));
                }
            }
        }

        public string Name
        {
            get => GetName();
            set => SetName(value);
        }

        public IBitmap? Icon { get; private set; }

        public bool CanEditName { get; }
        public bool CanRemove { get; }
        public bool CanDelete { get; }

        public ICommand BeginRenameCommand { get; }
        public ICommand EndRenameCommand { get; }
        public ICommand CreateShortcutCommand { get; }
        public ICommand BrowseFilesCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand DeleteCommand { get; }

        public AccurateVersion? Version => Instance?.Version;

        private FactorioInstanceViewModel(Manager manager)
        {
            if (manager is null) throw new ArgumentNullException(nameof(manager));
            _manager = manager;

            BeginRenameCommand = ReactiveCommand.Create(() => IsRenaming = true);
            EndRenameCommand = ReactiveCommand.Create(() => IsRenaming = false);
            CreateShortcutCommand = ReactiveCommand.CreateFromTask(CreateShortcutAsync);
            BrowseFilesCommand = ReactiveCommand.Create(BrowseFiles);
            RemoveCommand = ReactiveCommand.Create(Remove);
            DeleteCommand = ReactiveCommand.Create(Delete);
        }

        /// <summary>
        /// Use this constructor for already existing instances
        /// </summary>
        public FactorioInstanceViewModel(Manager manager, ManagedFactorioInstance instance)
            : this(manager)
        {
            if (instance is null) throw new ArgumentNullException(nameof(instance));
            _instance = instance;

            _isInstalled = true;

            IsExternal = instance.IsExternal();

            bool isSteam = instance.IsSteamInstance();
            if (isSteam) Icon = LoadIcon("Steam_Icon.png");
            else Icon = LoadIcon("Factorio_Icon.png");

            CanEditName = !isSteam;
            CanRemove = IsExternal && !isSteam;
            CanDelete = !IsExternal && !isSteam;
        }

        /// <summary>
        /// If this constructor is used the instance has to be created asyncroneously later
        /// </summary>
        public FactorioInstanceViewModel(Manager manager, LocationManager locations, bool download)
            : this(manager)
        {
            if (locations is null) throw new ArgumentNullException(nameof(locations));
            _locations = locations;

            _isInDownloadQueue = download;
            _isExtracting = !download;

            IsExternal = false; // Instance cannot be external if we are downloading or extracting it

            if (download) Icon = LoadIcon("Download_Icon.png");
            else Icon = LoadIcon("Package_Icon.png");

            CanEditName = true;
            CanRemove = false;
            CanDelete = true;
        }

        private static IBitmap LoadIcon(string name)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using var stream = assets.Open(new Uri($"avares://ModMyFactoryGUI/Assets/{name}"));
            return new Bitmap(stream);
        }

        private string GetName()
        {
            if (Instance is null)
            {
                // Doesn't have a name yet (either downloading or extracting)
                return string.Empty;
            }
            else
            {
                return Instance.GetName();
            }
        }

        private void SetName(string name)
        {
            if (Instance is null)
            {
                // Can't have a name yet (either downloading or extracting)
                return;
            }
            else
            {
                Instance.SetName(name);
                this.RaisePropertyChanged(nameof(Name));
            }
        }

        private void OnInstanceRemoved(EventArgs e)
            => InstanceRemoved?.Invoke(this, e);

        private string BuildArguments(ShortcutPropertiesViewModel vm)
        {
            int? modpackId = (vm.UseModpack && !(vm.SelectedModpack is null)) ? (int?)Program.GetModpackId(vm.SelectedModpack) : null;
            var savegamePath = vm.UseSavegame ? vm.SavegamePath : null;
            var customArgs = vm.UseCustomArgs ? vm.CustomArgs.Replace('"', '\'') : null;
            var opts = new StartGameOptions(Instance!.GetUniqueKey(), null, modpackId, null, savegamePath, customArgs, false, false, null);

            var args = Parser.Default.FormatCommandLine(opts);
#if !SELFCONTAINED
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                args = $"\"{assemblyPath}\" {args}";
            }
#endif
            return args;
        }

        private static string GetTargetPath()
        {
#if SELFCONTAINED
            var filePath = Assembly.GetExecutingAssembly().Location;
            var assemblyFile = new FileInfo(filePath);
            var fileName = Path.GetFileNameWithoutExtension(assemblyFile.Name);

#if WIN32
            filePath = Path.Combine(assemblyFile.Directory!.FullName, fileName + ".exe");
#else
            filePath = Path.Combine(assemblyFile.Directory!.FullName, fileName);
#endif

            return filePath;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var filePath = Assembly.GetExecutingAssembly().Location;
                var assemblyFile = new FileInfo(filePath);
                var fileName = Path.GetFileNameWithoutExtension(assemblyFile.Name);
                return Path.Combine(assemblyFile.Directory!.FullName, fileName + ".exe");
            }
            else
            {
                return "dotnet";
            }
#endif
        }

        public string? GetUniqueKey() => Instance?.GetUniqueKey();

        public async Task CreateShortcutAsync()
        {
            var vm = new ShortcutPropertiesViewModel();
            var dialog = View.CreateAndAttach(vm);
            var result = await dialog.ShowDialog<DialogResult>(App.Current.MainWindow);
            if (result == DialogResult.Ok)
            {
                var extensions = PlatformHelper.GetSymbolicLinkExtensions();
                var sfd = new SaveFileDialog();

                if (extensions.Length > 0)
                {
                    var filter = new FileDialogFilter();
                    filter.Extensions.AddRange(extensions);
                    filter.Name = (string)App.Current.Locales.GetResource("SymlinkFileType");
                    sfd.Filters.Add(filter);
                    sfd.DefaultExtension = extensions[0];
                }

                var path = await sfd.ShowAsync(App.Current.MainWindow);
                if (!string.IsNullOrEmpty(path))
                {
                    // We need to check this because in Linux the default extension is not automatically appended
                    if (string.IsNullOrEmpty(Path.GetExtension(path))) path += "." + extensions[0];

                    var args = BuildArguments(vm);
                    var target = GetTargetPath();
                    var iconPath = Path.Combine(Program.ApplicationDirectory.FullName, "Factorio_Icon.ico");
                    PlatformHelper.CreateSymbolicLink(path, target, args, iconPath);
                }
            }
        }

        public void BrowseFiles()
        {
            if (IsInstalled)
                PlatformHelper.OpenDirectory(Instance!.Directory);
        }

        public void Remove()
        {
            if (_instance!.IsSteamInstance()) throw new InvalidOperationException("Cannot remove the Steam instance");
            if (!IsExternal) throw new InvalidOperationException("Cannot remove an internal instance");

            // ToDo: ask user
            OnInstanceRemoved(EventArgs.Empty);
            if (Program.Settings.TryGet(SettingName.ExternalInstances, out List<string>? paths))
            {
                if (!(paths is null))
                {
                    for (int i = paths.Count - 1; i >= 0; i--)
                    {
                        if (FileHelper.PathsEqual(paths[i], _instance!.Directory.FullName))
                        {
                            paths.RemoveAt(i);
                            Program.Settings.Set(SettingName.ExternalInstances, paths);
                            break;
                        }
                    }
                }
            }
            _manager.RemoveInstance(_instance!);
        }

        public void Delete()
        {
            if (_instance!.IsSteamInstance()) throw new InvalidOperationException("Cannot delete the Steam instance");
            if (IsExternal) throw new InvalidOperationException("Cannot delete an external instance");

            // ToDo: ask user
            OnInstanceRemoved(EventArgs.Empty);
            _manager.RemoveInstance(_instance!);
            _instance!.Dispose();
            _instance.Directory.Delete(true);
        }

        public async Task<bool> TryCreateDownloadAsync(DownloadQueue downloadQueue, bool experimental)
        {
            if (downloadQueue is null) throw new ArgumentNullException(nameof(downloadQueue));

            // We can only download the latest available version (stable or experimental)
            // since that is all the API gives us. If other versions are desired the user
            // has to download them manually and use the extract option instead.
            var (stable, exp) = await DownloadApi.GetReleasesAsync();
            var toDownload = experimental ? exp : stable;

            if (toDownload.TryGetValue(FactorioBuild.Alpha, out var version))
            {
                var (success, username, token) = await App.Current.Credentials.TryLogInAsync();
                if (success.IsTrue())
                {
                    IsInDownloadQueue = true;

                    // Create a download job
                    var job = new DownloadFactorioJob(username, token,
                        version, FactorioBuild.Alpha, PlatformHelper.GetCurrentPlatform());

                    // Use an actual function instead of a lambda
                    // because we need to unsubscribe it again
                    void OnProgressChanged(object? sender, double progress)
                    {
                        IsInDownloadQueue = false;
                        IsDownloading = true;

                        DownloadProgress = progress;
                        this.RaisePropertyChanged(nameof(DownloadProgress));
                    }

                    // Start download and wait for it to complete
                    job.Progress.ProgressChanged += OnProgressChanged;
                    await downloadQueue.AddJobAsync(job);
                    job.Progress.ProgressChanged -= OnProgressChanged;

                    IsInDownloadQueue = false;
                    IsDownloading = false;

                    // After download we extract
                    if (job.Success)
                    {
                        // Update icon here because if we start directly with extract we already set it
                        var prevIcon = Icon;
                        Icon = LoadIcon("Package_Icon.png");
                        this.RaisePropertyChanged(nameof(Icon));
                        prevIcon?.Dispose();

                        return await TryCreateExtractAsync(job.File!, true);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public async Task<bool> TryCreateExtractAsync(FileInfo file, bool deleteOnError = false)
        {
            IsExtracting = true;
            string dirName = _locations!.GenerateNewFactorioDirectoryName();
            var (success, instance) = await Factorio.TryExtract(file, _locations.GetFactorioDir().FullName, dirName);
            IsExtracting = false;

            if (success)
            {
                IsInstalled = true;
                Instance = _manager.AddInstance(instance!);

                // Update to final icon
                var prevIcon = Icon;
                Icon = LoadIcon("Factorio_Icon.png");
                this.RaisePropertyChanged(nameof(Icon));
                prevIcon?.Dispose();

                return true;
            }
            else
            {
                if (deleteOnError && file.Exists) file.Delete();

                // ToDo: show error message
                return false;
            }
        }
    }
}
