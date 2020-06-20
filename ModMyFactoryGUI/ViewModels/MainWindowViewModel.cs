//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommandLine;
using ModMyFactory.Game;
using ModMyFactoryGUI.CommandLine;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Synchronization;
using ModMyFactoryGUI.Tasks;
using ModMyFactoryGUI.Tasks.Web;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class MainWindowViewModel : ScreenBase<MainWindow>
    {
        private readonly AboutWindowViewModel _aboutWindowViewModel = new AboutWindowViewModel();
        private readonly Progress<(DownloadJob, double)> _downloadProgress;
        private TabItem _selectedTab;
        private IMainViewModel _selectedViewModel;
        private FactorioInstanceViewModel _selectedFactorioInstance;

        public ICommand StartGameCommand { get; }

        public ICommand OpenFactorioDirCommand { get; }

        public ICommand OpenModDirCommand { get; }

        public ICommand NavigateToUrlCommand { get; }

        public ICommand OpenAboutWindowCommand { get; }

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

        public IReadOnlyCollection<IControl> FileMenuItems { get; private set; }

        public bool FileMenuVisible => FileMenuItems.Count > 0;

        public IReadOnlyCollection<IControl> EditMenuItems { get; private set; }

        public bool EditMenuVisible => EditMenuItems.Count > 0;

        public IReadOnlyCollection<IControl> ToolbarItems { get; private set; }

        public TabItem SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (value != _selectedTab)
                {
                    _selectedTab = value;
                    this.RaisePropertyChanged(nameof(SelectedTab));
                    SelectedViewModel = GetViewModel(_selectedTab);
                }
            }
        }

        public IMainViewModel SelectedViewModel
        {
            get => _selectedViewModel;
            private set
            {
                if (value != _selectedViewModel)
                {
                    _selectedViewModel = value;
                    this.RaisePropertyChanged(nameof(SelectedViewModel));

                    RebuildMenuItems(_selectedViewModel);
                    ReassignKeyBindings(_selectedViewModel);
                }
            }
        }

        public FactorioInstanceViewModel SelectedFactorioInstance
        {
            get => _selectedFactorioInstance;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedFactorioInstance, value, nameof(SelectedFactorioInstance));

                if (value is null) Program.Settings.Remove(SettingName.SelectedInstance);
                else Program.Settings.Set(SettingName.SelectedInstance, value.GetUniqueKey());
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

            _downloadProgress = new Progress<(DownloadJob, double)>(OnDownloadProgressChanged);
            DownloadQueue = new DownloadQueue(_downloadProgress);
            DownloadQueue.JobCompleted += OnDownloadJobCompleted;
            DownloadQueue.LengthChanged += OnDownloadQueueLengthChanged;
            DownloadQueue.StartQueue();
            App.ShuttingDown += (sender, e) => DownloadQueue.StopQueue();

            ManagerViewModel = new ManagerViewModel();
            OnlineModsViewModel = new OnlineModsViewModel(Program.Manager, DownloadQueue);
            FactorioViewModel = new FactorioViewModel(DownloadQueue);
            ExportViewModel = new ExportViewModel();
            SettingsViewModel = new SettingsViewModel(DownloadQueue);
            SelectedViewModel = ManagerViewModel;

            // Properties don't actually change but we need to refresh the formatters
            App.Current.Locales.UICultureChanged += (sender, e) =>
            {
                this.RaisePropertyChanged(nameof(DownloadDescription));
                this.RaisePropertyChanged(nameof(DownloadQueueLength));
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
                if (_selectedFactorioInstance is null)
                {
                    Program.Settings.Set(SettingName.SelectedInstance, _selectedFactorioInstance.GetUniqueKey());
                    Program.Settings.Save();
                }
            }

            GlobalContext.Current.MessageReceived += MessageReceivedHandler;
        }

        private void OnDownloadQueueLengthChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(DownloadQueueLength));
            this.RaisePropertyChanged(nameof(ShowDownloadQueueLength));
        }

        private void StartGame()
        {
            if (!(SelectedFactorioInstance is null))
            {
                var instance = SelectedFactorioInstance.Instance;
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

        private void OnDownloadJobCompleted(object sender, JobCompletedEventArgs<DownloadJob> e)
        {
            IsDownloading = false;
            this.RaisePropertyChanged(nameof(IsDownloading));
        }

        private IMainViewModel GetViewModel(TabItem tab)
        {
            if (tab.Content is IMainView view)
                return view.ViewModel;

            // This shouldn't happen
            throw new ArgumentException("Tab does not contain a valid view", nameof(tab));
        }

        private void RebuildMenuItems(IMainViewModel viewModel)
        {
            FileMenuItems = viewModel.FileMenuItems;
            this.RaisePropertyChanged(nameof(FileMenuItems));
            this.RaisePropertyChanged(nameof(FileMenuVisible));

            EditMenuItems = viewModel.EditMenuItems;
            this.RaisePropertyChanged(nameof(EditMenuItems));
            this.RaisePropertyChanged(nameof(EditMenuVisible));

            ToolbarItems = viewModel.ToolbarItems;
            this.RaisePropertyChanged(nameof(ToolbarItems));
        }

        private void ReassignKeyBindings(IMainViewModel viewModel)
        {
            if (!(AttachedView is null))
            {
                AttachedView.KeyBindings.Clear();

                var vms = Enumerable.Concat(viewModel.FileMenuViewModels, viewModel.EditMenuViewModels);
                foreach (var vm in vms)
                {
                    if (vm is ICommandItemViewModel cmdVM)
                    {
                        if (!(cmdVM.Gesture is null))
                        {
                            var keyBinding = new KeyBinding
                            {
                                Command = cmdVM.Command,
                                Gesture = cmdVM.Gesture
                            };

                            AttachedView.KeyBindings.Add(keyBinding);
                        }
                    }
                }
            }
        }

        private async void ImportPackagesAsync(IEnumerable<string> paths)
        {
            var helper = new ImportHelper(paths);
            await helper.ImportPackagesAsync();
        }

        private async Task EvaluateOptions(RunOptions options)
        {
            if (!(options.ImportList is null))
                await Dispatcher.UIThread.InvokeAsync(() => ImportPackagesAsync(options.ImportList));
        }

        private async void MessageReceivedHandler(object sender, MessageReceivedEventArgs e)
        {
            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.HelpWriter = Console.Out;
            });
            var parsedOptions = parser.ParseArguments<RunOptions>(e.Message);

            await parsedOptions.WithParsedAsync(EvaluateOptions);
        }

        private async void WindowOpenedHandler(object sender, EventArgs e)
        {
            var args = Environment.GetCommandLineArgs();

            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.HelpWriter = Console.Out;
            });
            var parsedOptions = parser.ParseArguments<RunOptions>(args);

            await parsedOptions.WithParsedAsync(EvaluateOptions);
        }

        protected override void OnViewChanged(EventArgs e)
        {
            base.OnViewChanged(e);
            ReassignKeyBindings(SelectedViewModel);
            AttachedView.Opened += WindowOpenedHandler;
        }
    }
}
