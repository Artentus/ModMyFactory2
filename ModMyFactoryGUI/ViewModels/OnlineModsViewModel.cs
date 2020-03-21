//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi;
using ModMyFactory.WebApi.Mods;
using ModMyFactoryGUI.Tasks.Web;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class OnlineModsViewModel : MainViewModelBase<OnlineModsView>
    {
        private readonly DownloadManager _downloadManager;
        private OnlineModViewModel _selectedMod;

        public bool ModsLoaded { get; private set; }

        public bool LoadingErrorOccurred { get; private set; }

        public string ErrorMessageKey { get; private set; }

        public IReadOnlyCollection<OnlineModViewModel> OnlineMods { get; private set; }

        public ICommand RefreshCommand { get; }

        public OnlineModViewModel SelectedMod
        {
            get => _selectedMod;
            set
            {
                if (value != _selectedMod)
                {
                    _selectedMod = value;
                    this.RaisePropertyChanged(nameof(SelectedMod));
                }
            }
        }

        public OnlineModsViewModel(DownloadManager downloadManager)
        {
            _downloadManager = downloadManager;
            RefreshCommand = ReactiveCommand.CreateFromTask(RefreshOnlineModsAsync);

            // This is fire-and-forget intentionally
            async void Refresh() => await RefreshOnlineModsAsync();
            Refresh();

            PropertyChanged += async (s, e) => await OnPropertyChanged(s, e);
        }

        private async Task OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(SelectedMod)) && !(SelectedMod is null))
                await SelectedMod.LoadExtendedInfoAsync();
        }

        private async Task<IReadOnlyCollection<OnlineModViewModel>> LoadOnlineModsAsync()
        {
            var page = await ModApi.RequestPageAsync();
            var result = new List<OnlineModViewModel>(page.Mods.Length);

            foreach (var info in page.Mods)
                result.Add(new OnlineModViewModel(info, _downloadManager));
            return result;
        }

        private void DisposeOnlineMods()
        {
            if (!(OnlineMods is null))
            {
                foreach (var onlineMod in OnlineMods)
                    onlineMod.Dispose();
            }
        }

        private async Task RefreshOnlineModsAsync()
        {
            ModsLoaded = false;
            this.RaisePropertyChanged(nameof(ModsLoaded));
            LoadingErrorOccurred = false;
            this.RaisePropertyChanged(nameof(LoadingErrorOccurred));
            SelectedMod = null;

            // Avoid memory leak
            DisposeOnlineMods();

            try
            {
                OnlineMods = await LoadOnlineModsAsync();
            }
            catch (ApiException ex)
            {
                if (ex is ConnectFailureException)
                {
                    // Connection error
                    LoadingErrorOccurred = true;
                    this.RaisePropertyChanged(nameof(LoadingErrorOccurred));
                    ErrorMessageKey = "ConnectionError_Message";
                    this.RaisePropertyChanged(nameof(ErrorMessageKey));
                }
                else if (ex is TimeoutException)
                {
                    // Timeout
                    LoadingErrorOccurred = true;
                    this.RaisePropertyChanged(nameof(LoadingErrorOccurred));
                    ErrorMessageKey = "TimeoutError_Message";
                    this.RaisePropertyChanged(nameof(ErrorMessageKey));
                }
                else
                {
                    // Server error
                    LoadingErrorOccurred = true;
                    this.RaisePropertyChanged(nameof(LoadingErrorOccurred));
                    ErrorMessageKey = "ServerError_Message";
                    this.RaisePropertyChanged(nameof(ErrorMessageKey));
                }
            }

            this.RaisePropertyChanged(nameof(OnlineMods));

            ModsLoaded = true;
            this.RaisePropertyChanged(nameof(ModsLoaded));
        }

        protected override List<IMenuItemViewModel> GetEditMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }

        protected override List<IMenuItemViewModel> GetFileMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }
    }
}
