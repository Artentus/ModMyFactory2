//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Threading;
using CommandLine;
using ModMyFactory.Game;
using ModMyFactory.Mods;
using ModMyFactoryGUI.CommandLine;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Synchronization;
using ModMyFactoryGUI.Tasks;
using ModMyFactoryGUI.Tasks.Web;
using ModMyFactoryGUI.Update;
using ModMyFactoryGUI.Views;
using Onova;
using Onova.Models;
using Onova.Services;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase<MainWindow>
    {
        private readonly AboutWindowViewModel _aboutWindowViewModel = new AboutWindowViewModel();
        private readonly Progress<(DownloadJob, double)> _downloadProgress;
        private TabItem? _selectedTab;
        private IMainViewModel? _selectedViewModel;
        private FactorioInstanceViewModel? _selectedFactorioInstance;

        public ICommand StartGameCommand { get; }

        public ICommand OpenFactorioDirCommand { get; }

        public ICommand OpenModDirCommand { get; }

        public ICommand NavigateToUrlCommand { get; }

        public ICommand OpenAboutWindowCommand { get; }

        public ICommand UpdateCommand { get; }

        public DownloadQueue DownloadQueue { get; }

        public bool IsDownloading { get; private set; }

        public string DownloadDescription { get; private set; }

        public double DownloadProgress { get; private set; }

        public int DownloadQueueLength => DownloadQueue.Length;

        public bool ShowDownloadQueueLength => DownloadQueueLength > 0;

        public IEnumerable<ThemeViewModel> AvailableThemes
            => App.Current.Themes.Select(t => new ThemeViewModel(t));

        public ManagerViewModel ManagerViewModel { get; }

        public OnlineModsViewModel OnlineModsViewModel { get; }

        public FactorioViewModel FactorioViewModel { get; }

        public ExportViewModel ExportViewModel { get; }

        public SettingsViewModel SettingsViewModel { get; }

        public TabItem? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (value != _selectedTab)
                {
                    _selectedTab = value;
                    this.RaisePropertyChanged(nameof(SelectedTab));

                    _selectedViewModel = GetViewModel(_selectedTab);
                    this.RaisePropertyChanged(nameof(SelectedViewModel));
                }
            }
        }

        public IMainViewModel? SelectedViewModel
        {
            get => _selectedViewModel;
            private set
            {
                if (value != _selectedViewModel)
                {
                    _selectedViewModel = value;
                    this.RaisePropertyChanged(nameof(SelectedViewModel));

                    _selectedTab = GetTab(_selectedViewModel);
                    this.RaisePropertyChanged(nameof(SelectedTab));
                }
            }
        }

        public CollectionView<FactorioInstanceViewModel> FactorioInstances { get; }

        public FactorioInstanceViewModel? SelectedFactorioInstance
        {
            get => _selectedFactorioInstance;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedFactorioInstance, value, nameof(SelectedFactorioInstance));

                if (value is null) Program.Settings.Remove(SettingName.SelectedInstance);
                else Program.Settings.Set(SettingName.SelectedInstance, value.GetUniqueKey()!);
                Program.Settings.Save();
            }
        }

        public IEnumerable<CultureViewModel> AvailableCultures
            => App.Current.Locales.AvailableCultures.Select(c => new CultureViewModel(c));

        public MainWindowViewModel()
        {
            StartGameCommand = ReactiveCommand.Create(StartGame);
            OpenFactorioDirCommand = ReactiveCommand.Create(OpenFactorioDir);
            OpenModDirCommand = ReactiveCommand.Create(OpenModDir);
            NavigateToUrlCommand = ReactiveCommand.Create<string>(NavigateToUrl);
            OpenAboutWindowCommand = ReactiveCommand.CreateFromTask(OpenAboutWindow);
            UpdateCommand = ReactiveCommand.CreateFromTask(async () => await UpdateAsync(true));

            _downloadProgress = new Progress<(DownloadJob, double)>(OnDownloadProgressChanged);
            DownloadDescription = string.Empty;
            DownloadQueue = new DownloadQueue(_downloadProgress);
            DownloadQueue.JobCompleted += OnDownloadJobCompleted;
            DownloadQueue.LengthChanged += OnDownloadQueueLengthChanged;
            DownloadQueue.StartQueue();
            App.ShuttingDown += (sender, e) => DownloadQueue.StopQueue();

            ManagerViewModel = new ManagerViewModel(0, DownloadQueue);
            OnlineModsViewModel = new OnlineModsViewModel(1, Program.Manager, DownloadQueue);
            FactorioViewModel = new FactorioViewModel(2, DownloadQueue);
            ExportViewModel = new ExportViewModel(3);
            SettingsViewModel = new SettingsViewModel(4, DownloadQueue);
            ManagerViewModel.BrowseModFamilyRequested += async (sender, e) =>
            {
                SelectedViewModel = OnlineModsViewModel;
                await OnlineModsViewModel.BrowseModFamilyAsync(e.Family);
            };

            // Properties don't actually change but we need to refresh the formatters
            App.Current.Locales.UICultureChanged += (sender, e) =>
            {
                this.RaisePropertyChanged(nameof(DownloadDescription));
                this.RaisePropertyChanged(nameof(DownloadQueueLength));
            };

            FactorioInstances = new CollectionView<FactorioInstanceViewModel>(FactorioViewModel.Instances, i => i.IsInstalled);
            foreach (var instance in FactorioViewModel.Instances)
            {
                if (!instance.IsInstalled)
                    instance.PropertyChanged += OnInstancePropertyChanged;
            }
            ((INotifyCollectionChanged)FactorioViewModel.Instances).CollectionChanged += (s, e) =>
            {
                if ((e.Action == NotifyCollectionChangedAction.Add) && !(e.NewItems is null))
                {
                    foreach (FactorioInstanceViewModel instance in e.NewItems)
                    {
                        if (!instance.IsInstalled)
                            instance.PropertyChanged += OnInstancePropertyChanged;
                    }
                }
            };

            if (Program.Settings.TryGet<string>(SettingName.SelectedInstance, out var key))
            {
                foreach (var instance in FactorioViewModel.Instances)
                {
                    if (instance.GetUniqueKey() == key)
                    {
                        _selectedFactorioInstance = instance;
                        break;
                    }
                }
            }
            if (_selectedFactorioInstance is null)
            {
                _selectedFactorioInstance = FactorioViewModel.Instances.FirstOrDefault();
                if (_selectedFactorioInstance is null) Program.Settings.Remove(SettingName.SelectedInstance);
                else Program.Settings.Set(SettingName.SelectedInstance, _selectedFactorioInstance.GetUniqueKey()!);
                Program.Settings.Save();
            }

            Program.SyncContext!.MessageReceived += MessageReceivedHandler;
        }

        private void OnInstancePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var instance = (FactorioInstanceViewModel)sender!;
            if ((e.PropertyName == nameof(FactorioInstanceViewModel.IsInstalled)) && instance.IsInstalled)
            {
                instance.PropertyChanged -= OnInstancePropertyChanged;
                FactorioInstances.Refresh();
            }
        }

        private void OnDownloadQueueLengthChanged(object? sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(DownloadQueueLength));
            this.RaisePropertyChanged(nameof(ShowDownloadQueueLength));
        }

        private void StartGame()
        {
            if (!(SelectedFactorioInstance is null))
            {
                var instance = SelectedFactorioInstance.Instance!;
                var modDir = Program.Locations.GetModDir(instance.Version);
                if (!modDir.Exists) modDir.Create();
                instance.Start(modDir);
            }
        }

        private void OpenFactorioDir()
        {
            var dir = Program.Locations.GetFactorioDir();
            PlatformHelper.OpenDirectory(dir);
        }

        private void OpenModDir()
        {
            var dir = Program.Locations.GetModDir();
            PlatformHelper.OpenDirectory(dir);
        }

        private void NavigateToUrl(string url)
            => PlatformHelper.OpenWebUrl(url);

        private async Task OpenAboutWindow()
        {
            var window = View.CreateAndAttach(_aboutWindowViewModel);
            await window.ShowDialog(AttachedView);
        }

        private async Task<(bool, TagVersion?, string?, string?)> PrepareUpdateAsync(bool includePrerelease, bool showResultMessage)
        {
            try
            {
                Log.Information("Searching for updates...");

                var updateTask = UpdateApi.CheckForUpdateAsync(includePrerelease);
                var changelogTask = UpdateApi.RequestChangelogAsync();

                string title = (string)App.Current.Locales.GetResource("Update_Title");
                string message = (string)App.Current.Locales.GetResource("UpdateSearch_Message");
                await ProgressDialog.Show(title, message, Task.WhenAll(updateTask, changelogTask), AttachedView!);

                var (available, version, url) = updateTask.Result;
                return (available, version, url, changelogTask.Result);
            }
            catch (WebException ex)
            {
                if (showResultMessage) await MessageHelper.ShowMessageForWebException(ex);
                return (false, null, null, null);
            }
            catch (HttpRequestException ex)
            {
                if (showResultMessage) await MessageHelper.ShowMessageForHttpException(ex);
                return (false, null, null, null);
            }
        }

        private async Task<bool> DownloadUpdateAsync(string url, string fileName)
        {
            try
            {
                Log.Verbose("Downloading update package from '{0}' to '{1}'", url, fileName);

                using var wc = new WebClient();
                var downloadTask = wc.DownloadFileTaskAsync(url, fileName);

                string title = (string)App.Current.Locales.GetResource("Update_Title");
                string message = (string)App.Current.Locales.GetResource("UpdateDownload_Message");
                await ProgressDialog.Show(title, message, downloadTask, AttachedView!);
                return true;
            }
            catch (WebException ex)
            {
                await MessageHelper.ShowMessageForWebException(ex);
                return false;
            }
        }

        private async Task UpdateAsync(bool showResultMessage)
        {
            if (DownloadQueue.IsJobInProgress)
            {
                // Updating while downloads are in progress could result in invalid states
                await Messages.UpdateWhileDownloading.Show();
            }
            else
            {
                bool includePrerelease = VersionStatistics.AppVersion.IsPrerelease || Program.Settings.Get(SettingName.UpdatePrerelease, false);
                var (available, version, url, changelog) = await PrepareUpdateAsync(includePrerelease, showResultMessage);
                if (available)
                {
                    Log.Information("Found update version {0}", version);

                    var vm = new UpdateWindowViewModel(version!, changelog!);
                    var window = View.CreateAndAttach(vm);
                    await window.ShowDialog(AttachedView);

                    string fileName = Path.Combine(Program.TemporaryDirectory.FullName, Path.GetFileName(url!));
                    if ((vm.DialogResult == DialogResult.Ok) && (await DownloadUpdateAsync(url!, fileName)))
                    {
                        var metaData = AssemblyMetadata.FromAssembly(Assembly.GetAssembly(typeof(App))!);
                        var resolver = new ManualPackageResolver(fileName, version!);
                        var updateManager = new UpdateManager(metaData, resolver, new ZipPackageExtractor());

                        var result = await updateManager.CheckForUpdatesAsync();
                        await updateManager.PrepareUpdateAsync(result.LastVersion!);
                        string args = "--no-update";
                        if (!string.IsNullOrEmpty(Program.RestartArgs)) args += " " + Program.RestartArgs;
                        updateManager.LaunchUpdater(result.LastVersion!, true, args);

                        Log.Information("Shutting down for update");
                        AttachedView!.Close();
                    }
                }
                else
                {
                    Log.Information("No updates available");
                    if (showResultMessage) await Messages.NoUpdateFound.Show();
                }
            }
        }

        private void OnDownloadProgressChanged((DownloadJob, double) progress)
        {
            if (!IsDownloading)
            {
                IsDownloading = true;
                this.RaisePropertyChanged(nameof(IsDownloading));
                DownloadDescription = progress.Item1.Description;
                this.RaisePropertyChanged(nameof(DownloadDescription));
            }

            DownloadProgress = progress.Item2;
            this.RaisePropertyChanged(nameof(DownloadProgress));
        }

        private async Task AddModUpdateAsync(UpdateModJob job)
        {
            var fileHash = await job.File!.ComputeSHA1Async();
            var targetHash = job.Release.Checksum;
            if (fileHash == targetHash)
            {
                var (success, mod) = await Mod.TryLoadAsync(job.File!);
                if (success)
                {
                    // Mod successfully downloaded
                    Program.Manager.AddMod(mod!);
                    Log.Information($"Mod {mod!.Name} version {mod!.Version} successfully loaded from mod portal");

                    if (job.Replace)
                    {
                        // We have to find all older versions of the updated mod and replace them
                        foreach (var modpack in Program.Modpacks)
                        {
                            foreach (var old in modpack.Mods)
                            {
                                if (string.Equals(old.Name, job.Info.Family.FamilyName, StringComparison.InvariantCulture))
                                {
                                    modpack.Remove(old); // Ok because we break
                                    modpack.Add(mod);
                                    break;
                                }
                            }
                        }

                        if (job.RemoveOld)
                        {
                            if (mod.Family is null) throw new InvalidOperationException(); // Should not happen

                            var others = mod.Family.Where(m => !object.ReferenceEquals(m, mod)).ToArray();
                            foreach (var other in others)
                            {
                                Program.Manager.RemoveMod(other);
                                other.Dispose();
                                other.File?.Delete();
                            }
                        }
                    }
                }
                else
                {
                    await Messages.InvalidModFile(job.File!).Show();
                }
            }
            else
            {
                await Messages.FileIntegrityError(job.File!, targetHash, fileHash).Show();
            }
        }

        private async void OnDownloadJobCompleted(object? sender, JobCompletedEventArgs<DownloadJob> e)
        {
            if (e.Job is UpdateModJob job)
                await AddModUpdateAsync(job);

            if (!DownloadQueue.IsJobInProgress)
            {
                IsDownloading = false;
                this.RaisePropertyChanged(nameof(IsDownloading));
            }
        }

        private IMainViewModel? GetViewModel(TabItem? tab)
        {
            if (tab is null) return null;

            if (tab.Content is IMainView view)
                return view.ViewModel!;

            // This shouldn't happen
            throw new ArgumentException("Tab does not contain a valid view", nameof(tab));
        }

        private TabItem? GetTab(IMainViewModel? vm)
        {
            if (vm is null) return null;

            var tabControl = AttachedView!.FindControl<TabControl>("MainTabs");
            return tabControl.ItemContainerGenerator.ContainerFromIndex(vm.TabIndex) as TabItem;
        }

        private async Task ImportPackagesAsync(IEnumerable<string> paths)
        {
            using var helper = new ImportHelper(paths);
            await helper.ImportPackagesAsync();
        }

        private async Task EvaluateOptions(RunOptions options)
        {
            if (!(options.ImportList is null))
            {
                if (Dispatcher.UIThread.CheckAccess()) await ImportPackagesAsync(options.ImportList);
                else await Dispatcher.UIThread.InvokeAsync(async () => await ImportPackagesAsync(options.ImportList));
            }
        }

        private async void MessageReceivedHandler(object? sender, MessageReceivedEventArgs e)
        {
            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.HelpWriter = Console.Out;
            });
            var parsedOptions = parser.ParseArguments<RunOptions>(e.Message);

            await parsedOptions.WithParsedAsync(EvaluateOptions);
        }

#if !DEBUG
        private async Task StartupUpdate()
        {
            if (Program.Settings.Get(SettingName.UpdateOnStartup, true))
            {
                if (Program.SkipUpdateCheck)
                {
                    Log.Verbose("Skipping startup update check due to command line");
                }
                else
                {
                    await UpdateAsync(false);
                }
            }
        }
#endif

        private async void WindowOpenedHandler(object? sender, EventArgs e)
        {
            var args = Environment.GetCommandLineArgs();

            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.HelpWriter = Console.Out;
            });
            var parsedOptions = parser.ParseArguments<RunOptions>(args);

            await parsedOptions.WithParsedAsync(EvaluateOptions);

#if !DEBUG
            await StartupUpdate();
#endif
        }

        protected override void OnViewChanged(EventArgs e)
        {
            base.OnViewChanged(e);
            AttachedView!.Opened += WindowOpenedHandler;
        }
    }
}
