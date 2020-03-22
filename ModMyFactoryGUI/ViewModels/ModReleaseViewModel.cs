//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.WebApi.Mods;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Tasks.Web;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModReleaseViewModel : ReactiveObject
    {
        private readonly string _modDisplayName;
        private readonly DownloadQueue _downloadQueue;

        public ModReleaseInfo Info { get; }

        public ICommand DownloadCommand { get; }

        public ICommand DeleteCommand { get; }

        public bool IsInstalled => false; // ToDo implement

        public AccurateVersion Version => Info.Version;

        public AccurateVersion FactorioVersion => Info.Info.FactorioVersion;

        public ModReleaseViewModel(ModReleaseInfo info, string modDisplayName, DownloadQueue downloadQueue)
        {
            Info = info;
            _modDisplayName = modDisplayName;
            _downloadQueue = downloadQueue;

            DownloadCommand = ReactiveCommand.CreateFromTask(Download);
            DeleteCommand = ReactiveCommand.Create(Delete);
        }

        public async Task Download()
        {
            if (!IsInstalled)
            {
                var (success, username, token) = await App.Current.Credentials.TryLogInAsync();
                if (success.IsTrue())
                {
                    var job = new DownloadModReleaseJob(Info, _modDisplayName, username, token);
                    _downloadQueue.AddJob(job);
                }
            }
        }

        public void Delete()
        {
            if (IsInstalled)
            {
                // ToDo implement
            }
        }
    }
}
