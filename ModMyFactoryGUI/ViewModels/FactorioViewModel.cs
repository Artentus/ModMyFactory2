//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.Tasks.Web;
using ModMyFactoryGUI.Views;
using System.Collections.Generic;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class FactorioViewModel : MainViewModelBase<FactorioView>
    {
        private readonly DownloadManager _downloadManager;

        public FactorioViewModel(DownloadManager downloadManager)
            => _downloadManager = downloadManager;

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
