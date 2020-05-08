//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory;
using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using ModMyFactory.WebApi.Mods;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Tasks;
using ModMyFactoryGUI.Tasks.Web;
using ReactiveUI;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModReleaseViewModel : ReactiveObject, IDisposable
    {
        private readonly string _modName, _modDisplayName;
        private readonly DownloadQueue _downloadQueue;
        private ModManager _modManager;
        private bool _isInstalled;

        private bool isDisposed = false;

        public ModReleaseInfo Info { get; }

        public ICommand DownloadCommand { get; }

        public ICommand DeleteCommand { get; }

        public bool IsInstalled
        {
            get => _isInstalled;
            private set
            {
                if (value != _isInstalled)
                {
                    _isInstalled = value;
                    this.RaisePropertyChanged(nameof(IsInstalled));
                }
            }
        }

        public AccurateVersion Version => Info.Version;

        public AccurateVersion FactorioVersion => Info.Info.FactorioVersion;

        public ModReleaseViewModel(ModReleaseInfo info, string modName, string modDisplayName, Manager manager, DownloadQueue downloadQueue)
        {
            Info = info;
            _modName = modName;
            _modDisplayName = modDisplayName;
            _downloadQueue = downloadQueue;

            DownloadCommand = ReactiveCommand.CreateFromTask(Download);
            DeleteCommand = ReactiveCommand.Create(Delete);

            if (manager.TryGetModManager(FactorioVersion, out _modManager))
            {
                _isInstalled = _modManager.Contains(modName, info.Version);
                _modManager.CollectionChanged += ModManagerCollectionChangedHandler;
            }
            else
            {
                _modManager = null;
                _isInstalled = false;

                void ModManagerCreatedHandler(object sender, ModManagerCreatedEventArgs e)
                {
                    if (e.ModManager.FactorioVersion == FactorioVersion)
                    {
                        _modManager = e.ModManager;
                        _modManager.CollectionChanged += ModManagerCollectionChangedHandler;
                        manager.ModManagerCreated -= ModManagerCreatedHandler;
                    }
                }

                manager.ModManagerCreated += ModManagerCreatedHandler;
            }
        }

        ~ModReleaseViewModel()
        {
            Dispose(false);
        }

        private void ModManagerCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newMods = e.NewItems.Cast<Mod>();
                    foreach (var mod in newMods)
                    {
                        if (mod.Name == _modName && mod.Version == Version)
                            IsInstalled = true;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var oldMods = e.OldItems.Cast<Mod>();
                    foreach (var mod in oldMods)
                    {
                        if (mod.Name == _modName && mod.Version == Version)
                            IsInstalled = false;
                    }
                    break;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (!(_modManager is null)) _modManager.CollectionChanged -= ModManagerCollectionChangedHandler;
                isDisposed = true;
            }
        }

        public async Task Download()
        {
            if (!IsInstalled)
            {
                IsInstalled = true;

                var (success, username, token) = await App.Current.Credentials.TryLogInAsync();
                if (success.IsTrue())
                {
                    var job = new DownloadModReleaseJob(Info, _modDisplayName, username, token);

                    void JobCompletedHandler(object sender, JobCompletedEventArgs<DownloadJob> e)
                    {
                        if (object.ReferenceEquals(e.Job, job)) IsInstalled = e.Success;
                        _downloadQueue.JobCompleted -= JobCompletedHandler;
                    }

                    _downloadQueue.JobCompleted += JobCompletedHandler;
                    _downloadQueue.AddJob(job);
                }
                else
                {
                    IsInstalled = false;
                }
            }
        }

        public void Delete()
        {
            if (IsInstalled && !(_modManager is null))
            {
                // ToDo: ask for confirmation
                if (_modManager.Contains(_modName, Info.Version, out var mod))
                {
                    _modManager.Remove(mod);
                    mod.File.Delete();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
