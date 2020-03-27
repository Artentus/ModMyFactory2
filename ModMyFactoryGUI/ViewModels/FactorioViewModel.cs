//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.Tasks.Web;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class FactorioViewModel : MainViewModelBase<FactorioView>
    {
        private readonly DownloadQueue _downloadQueue;
        private readonly ObservableCollection<FactorioInstanceViewModel> _instances;

        public ReadOnlyObservableCollection<FactorioInstanceViewModel> Instances { get; }

        public ICommand DownloadCommand { get; }

        public FactorioViewModel(DownloadQueue downloadQueue)
        {
            _downloadQueue = downloadQueue;
            DownloadCommand = ReactiveCommand.CreateFromTask<bool>(DownloadAsync);

            _instances = new ObservableCollection<FactorioInstanceViewModel>();
            Instances = new ReadOnlyObservableCollection<FactorioInstanceViewModel>(_instances);
        }

        private async Task DownloadAsync(bool experimental)
        {
            var vm = new FactorioInstanceViewModel(Program.Manager, Program.Locations, true);
            _instances.Add(vm);

            if (!await vm.TryCreateDownloadAsync(_downloadQueue, experimental))
                _instances.Remove(vm);
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
