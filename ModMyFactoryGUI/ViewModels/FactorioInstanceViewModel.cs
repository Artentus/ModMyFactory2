//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Game;
using ModMyFactory.WebApi.Factorio;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Tasks.Web;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class FactorioInstanceViewModel : ReactiveObject
    {
        private static readonly Dictionary<string, string> NameTable;

        private readonly Manager _manager;
        private readonly LocationManager _locations;
        private bool _isInDownloadQueue, _isDownloading, _isExtracting, _isInstalled, _isRenaming;
        private ManagedFactorioInstance _instance;

        public event EventHandler InstanceRemoved;

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
                if (this.RaiseAndSetIfChanged(ref _isRenaming, value, nameof(IsRenaming)) && !value)
                {
                    Program.Settings.Set(SettingName.FactorioNameTable, NameTable);
                    Program.Settings.Save();
                }
            }
        }

        public double DownloadProgress { get; private set; }

        // True if the instance does not reside inside the managed directory
        public bool IsExternal { get; }

        public ManagedFactorioInstance Instance
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
            get => GetName(this);
            set => SetName(this, value);
        }

        public IBitmap Icon { get; private set; }

        public bool CanEditName { get; }
        public bool CanRemove { get; }
        public bool CanDelete { get; }

        public ICommand BeginRenameCommand { get; }
        public ICommand EndRenameCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand DeleteCommand { get; }

        public AccurateVersion? Version => Instance?.Version;

        private FactorioInstanceViewModel(Manager manager)
        {
            if (manager is null) throw new ArgumentNullException(nameof(manager));
            _manager = manager;

            BeginRenameCommand = ReactiveCommand.Create(() => IsRenaming = true);
            EndRenameCommand = ReactiveCommand.Create(() => IsRenaming = false);
            RemoveCommand = ReactiveCommand.Create(Remove);
            DeleteCommand = ReactiveCommand.Create(Delete);
        }

        static FactorioInstanceViewModel()
        {
            if (!Program.Settings.TryGet(SettingName.FactorioNameTable, out NameTable))
                NameTable = new Dictionary<string, string>();
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

            IsExternal = IsInstanceExternal(instance);

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

        private static bool IsInstanceExternal(ManagedFactorioInstance instance)
        {
            var instDir = instance.Directory.Parent;
            var managedDir = Program.Locations.GetFactorioDir();
            return !FileHelper.DirectoriesEqual(instDir, managedDir);
        }

        private static string GetNameKey(FactorioInstanceViewModel vm)
        {
            var instance = vm.Instance;
            if (vm.IsExternal)
            {
                // We use the full path of the instance as unique key
                string result = instance.Directory.FullName.Trim().ToLower();

                // We have to sanitize the path to make sure it's a proper unique key
                result = result.Replace('/', '_');
                result = result.Replace('\\', '_');
                if (result.EndsWith("_")) result = result.Substring(0, result.Length - 1);

                return result;
            }
            else
            {
                // The directory name is already a unique key, no need to use the full path
                return instance.Directory.Name;
            }
        }

        private static string GetName(FactorioInstanceViewModel vm)
        {
            var instance = vm.Instance;
            if (instance is null)
            {
                // Doesn't have a name yet (either downloading or extracting)
                return string.Empty;
            }
            else
            {
                if (instance.IsSteamInstance())
                {
                    // Steam instance has fixed name
                    return "Steam";
                }
                else
                {
                    string key = GetNameKey(vm);
                    return NameTable.GetValue(key, "Factorio");
                }
            }
        }

        private static void SetName(FactorioInstanceViewModel vm, string name)
        {
            var instance = vm.Instance;
            if (instance is null)
            {
                // Can't have a name yet (either downloading or extracting)
                return;
            }
            else
            {
                if (instance.IsSteamInstance())
                {
                    // Steam instance has fixed name
                    return;
                }
                else
                {
                    string key = GetNameKey(vm);
                    NameTable[key] = name;
                }
            }
        }

        private void OnInstanceRemoved(EventArgs e)
            => InstanceRemoved?.Invoke(this, e);

        public string GetUniqueKey() => GetNameKey(this);

        public void Remove()
        {
            if (_instance.IsSteamInstance()) throw new InvalidOperationException("Cannot remove the Steam instance");
            if (!IsExternal) throw new InvalidOperationException("Cannot remove an internal instance");

            // ToDo: ask user
            OnInstanceRemoved(EventArgs.Empty);
            if (Program.Settings.TryGet(SettingName.ExternalInstances, out List<string> paths))
            {
                for (int i = paths.Count - 1; i >= 0; i--)
                {
                    if (FileHelper.PathsEqual(paths[i], _instance.Directory.FullName))
                    {
                        paths.RemoveAt(i);
                        Program.Settings.Set(SettingName.ExternalInstances, paths);
                        break;
                    }
                }
            }
            _manager.RemoveInstance(_instance);
        }

        public void Delete()
        {
            if (_instance.IsSteamInstance()) throw new InvalidOperationException("Cannot delete the Steam instance");
            if (IsExternal) throw new InvalidOperationException("Cannot delete an external instance");

            // ToDo: ask user
            OnInstanceRemoved(EventArgs.Empty);
            _manager.RemoveInstance(_instance);
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
                    void OnProgressChanged(object sender, double progress)
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
                        prevIcon.Dispose();

                        return await TryCreateExtractAsync(job.File, true);
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
            string dirName = _locations.GenerateNewFactorioDirectoryName();
            var (success, instance) = await Factorio.TryExtract(file, _locations.GetFactorioDir().FullName, dirName);
            IsExtracting = false;

            if (success)
            {
                IsInstalled = true;
                Instance = _manager.AddInstance(instance);

                // Update to final icon
                var prevIcon = Icon;
                Icon = LoadIcon("Factorio_Icon.png");
                this.RaisePropertyChanged(nameof(Icon));
                prevIcon.Dispose();

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
