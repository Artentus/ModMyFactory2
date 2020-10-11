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
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Tasks;
using ModMyFactoryGUI.Tasks.Web;
using ReactiveUI;
using Serilog;
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
            DeleteCommand = ReactiveCommand.CreateFromTask(Delete);

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
                var (success, username, token) = await App.Current.Credentials.TryLogInAsync();
                if (success.IsTrue())
                {
                    IsInstalled = true;
                    var job = new DownloadModReleaseJob(Info, _modDisplayName, username, token);

                    async void JobCompletedHandler(object sender, JobCompletedEventArgs<DownloadJob> e)
                    {
                        if (object.ReferenceEquals(e.Job, job))
                        {
                            IsInstalled = e.Success;
                            _downloadQueue.JobCompleted -= JobCompletedHandler;

                            if (e.Success)
                            {
                                var fileHash = await job.File.ComputeSHA1Async();
                                var targetHash = job.Release.Checksum;
                                if (fileHash == targetHash)
                                {
                                    var (success, mod) = await Mod.TryLoadAsync(job.File);
                                    if (success)
                                    {
                                        // Mod successfully downloaded
                                        _modManager.Add(mod);
                                        Log.Information($"Mod {mod.Name} version {mod.Version} successfully loaded from mod portal");
                                    }
                                    else
                                    {
                                        await Messages.InvalidModFile(job.File).Show();
                                    }
                                }
                                else
                                {
                                    await Messages.FileIntegrityError(job.File, targetHash, fileHash).Show();
                                }
                            }
                        }
                    }

                    _downloadQueue.JobCompleted += JobCompletedHandler;
                    _downloadQueue.AddJob(job);
                }
            }
        }

        public async Task Delete()
        {
            if (IsInstalled && !(_modManager is null))
            {
                if (_modManager.Contains(_modName, Info.Version, out var mod))
                {
                    var title = (string)App.Current.Locales.GetResource("DeleteConfirm_Title");
                    var message = string.Format((string)App.Current.Locales.GetResource("DeleteConfirm_Mod_Message"), mod.DisplayName, mod.Version);
                    var result = await MessageBox.Show(title, message, MessageKind.Question, DialogOptions.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        _modManager.Remove(mod);
                        mod.Dispose();
                        mod.File.Delete();
                    }
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
