//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using ModMyFactory.Mods;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.MVVM;
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

        public ICommand NavigateToUrlCommand { get; }

        public ICommand OpenAboutWindowCommand { get; }

        public DownloadManager DownloadManager { get; }

        public IEnumerable<ThemeViewModel> AvailableThemes
            => App.Current.ThemeManager.Select(t => new ThemeViewModel(t));

        public ManagerViewModel ManagerViewModel { get; }

        public OnlineModsViewModel OnlineModsViewModel { get; }

        public FactorioViewModel FactorioViewModel { get; }

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
                }
            }
        }

        public IEnumerable<CultureViewModel> AvailableCultures
            => App.Current.LocaleManager.AvailableCultures.Select(c => new CultureViewModel(c));

        public MainWindowViewModel()
        {
            NavigateToUrlCommand = ReactiveCommand.Create<string>(NavigateToUrl);
            OpenAboutWindowCommand = ReactiveCommand.CreateFromTask(OpenAboutWindow);

            _downloadProgress = new Progress<(DownloadJob, double)>(OnDownloadProgressChanged);
            DownloadManager = new DownloadManager(_downloadProgress);
            DownloadManager.JobCompleted += OnDownloadJobCompleted;

            ManagerViewModel = new ManagerViewModel();
            OnlineModsViewModel = new OnlineModsViewModel(DownloadManager);
            FactorioViewModel = new FactorioViewModel(DownloadManager);
            SettingsViewModel = new SettingsViewModel();
            SelectedViewModel = ManagerViewModel;
        }

        private void NavigateToUrl(string url)
            => PlatformHelper.OpenWebUrl(url);

        private async Task OpenAboutWindow()
        {
            var window = View.CreateAndAttach(_aboutWindowViewModel);
            await window.ShowDialog(AttachedView);
        }

        private async void OnDownloadProgressChanged((DownloadJob, double) progress)
        {
        }

        private async Task OnDownloadModRelease(DownloadModReleaseJob job)
        {
            var fileHash = await job.File.ComputeSHA1Async();
            var targetHash = SHA1Hash.Parse(job.Release.Checksum);
            if (fileHash == targetHash)
            {
                var (success, mod) = await Mod.TryLoadAsync(job.File);
                if (success)
                {
                    // Mod successfully downloaded

                    return;
                }
            }

            // File is invalid
        }

        private async void OnDownloadJobCompleted(object sender, JobCompletedEventArgs<DownloadJob> e)
        {
            switch (e.Job)
            {
                case DownloadModReleaseJob j:
                    await OnDownloadModRelease(j);
                    break;
            }
        }

        private IMainViewModel GetViewModel(TabItem tab)
        {
            if (tab.Content is IMainView view)
                return view.ViewModel;

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
    }
}
