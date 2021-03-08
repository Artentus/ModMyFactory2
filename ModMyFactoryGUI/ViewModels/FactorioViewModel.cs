//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using ModMyFactory.Game;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.Tasks.Web;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class FactorioViewModel : MainViewModelBase<FactorioView>
    {
        private readonly DownloadQueue _downloadQueue;
        private readonly ObservableCollection<FactorioInstanceViewModel> _instances;

        public ReadOnlyObservableCollection<FactorioInstanceViewModel> Instances { get; }

        public ICommand ImportCommand { get; }

        public ICommand DownloadCommand { get; }

        public FactorioViewModel(int tabIndex, DownloadQueue downloadQueue)
            : base(tabIndex)
        {
            _downloadQueue = downloadQueue;
            ImportCommand = ReactiveCommand.CreateFromTask<bool>(ImportAsync);
            DownloadCommand = ReactiveCommand.CreateFromTask<bool>(DownloadAsync);

            _instances = new ObservableCollection<FactorioInstanceViewModel>();
            Instances = new ReadOnlyObservableCollection<FactorioInstanceViewModel>(_instances);
            ReloadInstances();
        }

        private void ReloadInstances()
        {
            _instances.Clear();
            foreach (var instance in Program.Manager.ManagedInstances)
            {
                var vm = new FactorioInstanceViewModel(Program.Manager, instance);
                vm.InstanceRemoved += OnInstanceRemoved;
                _instances.Add(vm);
            }
        }

        private void OnInstanceRemoved(object? sender, EventArgs e)
        {
            var vm = (FactorioInstanceViewModel)sender!;
            vm.InstanceRemoved -= OnInstanceRemoved;
            _instances.Remove(vm);
        }

        private async Task ImportArchiveAsync()
        {
            var extensions = PlatformHelper.GetFactorioArchiveExtensions();
            var filter = new FileDialogFilter();
            filter.Extensions.AddRange(extensions);
            filter.Name = (string)App.Current.Locales.GetResource("ArchiveFileType");

            var ofd = new OpenFileDialog { AllowMultiple = false };
            ofd.Filters.Add(filter);

            var paths = await ofd.ShowAsync(App.Current.MainWindow);
            if (!(paths is null) && (paths.Length > 0))
            {
                var file = new FileInfo(paths[0]);
                if (file.Exists)
                {
                    var vm = new FactorioInstanceViewModel(Program.Manager, Program.Locations, false);
                    _instances.Add(vm);

                    if (!await vm.TryCreateExtractAsync(file))
                        _instances.Remove(vm);
                }
            }
        }

        private async Task ImportInstanceAsync()
        {
            var ofd = new OpenSingleFileDialog
            {
                FilterName = "Factorio",
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "factorio.exe"
                : "factorio"
            };

            var path = await ofd.ShowAsync(App.Current.MainWindow);
            if (!(path is null))
            {
                var file = new FileInfo(path);
                if (file.Exists)
                {
                    var dir = file.Directory?.Parent?.Parent;
                    if (dir is null)
                    {
                        // Invalid file selected
                    }
                    else
                    {
                        var (success, instance) = await Factorio.TryLoadAsync(dir);
                        if (success)
                        {
                            var managed = Program.Manager.AddInstance(instance!);

                            if (!Program.Settings.TryGet(SettingName.ExternalInstances, out List<string>? instPaths) || (instPaths is null))
                                instPaths = new List<string>();
                            instPaths.Add(dir.FullName);
                            Program.Settings.Set(SettingName.ExternalInstances, instPaths);
                            await Program.Settings.SaveAsync();

                            var vm = new FactorioInstanceViewModel(Program.Manager, managed);
                            _instances.Add(vm);
                        }
                        else
                        {
                            // Loading failed
                        }
                    }
                }
            }
        }

        private Task ImportAsync(bool archive)
        {
            if (archive) return ImportArchiveAsync();
            else return ImportInstanceAsync();
        }

        private async Task DownloadAsync(bool experimental)
        {
            var vm = new FactorioInstanceViewModel(Program.Manager, Program.Locations, true);
            _instances.Add(vm);

            if (!await vm.TryCreateDownloadAsync(_downloadQueue, experimental))
                _instances.Remove(vm);
        }
    }
}
