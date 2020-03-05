//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi.Mods;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class OnlineModsViewModel : MainViewModelBase<OnlineModsView>
    {
        private OnlineModViewModel _selectedMod;

        public bool ModsLoaded { get; private set; }

        public IReadOnlyCollection<OnlineModViewModel> OnlineMods { get; private set; }

        public OnlineModViewModel SelectedMod
        {
            get => _selectedMod;
            set => this.RaiseAndSetIfChanged(ref _selectedMod, value, nameof(SelectedMod));
        }

        public OnlineModsViewModel()
        {
            // This is fire-and-forget intentionally
#pragma warning disable CS4014
            RefreshOnlineModsAsync();
#pragma warning restore CS4014
        }

        private async Task<IReadOnlyCollection<OnlineModViewModel>> LoadOnlineModsAsync()
        {
            var page = await ModApi.RequestPageAsync();
            var result = new List<OnlineModViewModel>(page.Mods.Length);

            foreach (var info in page.Mods)
                result.Add(new OnlineModViewModel(info));
            return result;
        }

        private async Task RefreshOnlineModsAsync()
        {
            ModsLoaded = false;
            this.RaisePropertyChanged(nameof(ModsLoaded));
            SelectedMod = null;

            // ToDo: Error handling, since the caller doesn't
            OnlineMods = await LoadOnlineModsAsync();
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
