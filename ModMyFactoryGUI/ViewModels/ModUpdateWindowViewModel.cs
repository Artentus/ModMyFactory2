//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModUpdateWindowViewModel : ViewModelBase<ModUpdateWindow>
    {
        public CollectionView<ModUpdateViewModel> Updates { get; }

        public ICommand UpdateCommand { get; }

        public ICommand CancelCommand { get; }

        public DialogResult DialogResult { get; private set; }

        public bool ReplaceUpdates
        {
            get => Program.Settings.Get(SettingName.ReplaceUpdates, true);
            set
            {
                Program.Settings.Set(SettingName.ReplaceUpdates, value);
                Program.Settings.Save();

                this.RaisePropertyChanged(nameof(ReplaceUpdates));
            }
        }

        public bool RemoveOldMods
        {
            get => Program.Settings.Get(SettingName.RemoveOldMods, true);
            set
            {
                Program.Settings.Set(SettingName.RemoveOldMods, value);
                Program.Settings.Save();

                this.RaisePropertyChanged(nameof(RemoveOldMods));
            }
        }

        public ModUpdateWindowViewModel(IReadOnlyDictionary<AccurateVersion, List<ModUpdateInfo>> updates)
        {
            DialogResult = DialogResult.None;
            UpdateCommand = ReactiveCommand.Create(Update);
            CancelCommand = ReactiveCommand.Create(Cancel);

            var updateVMs = new List<ModUpdateViewModel>(updates.Count);
            foreach (var kvp in updates)
                updateVMs.Add(new ModUpdateViewModel(kvp.Key, kvp.Value));

            static int CompareVersions(ModUpdateViewModel first, ModUpdateViewModel second)
                => second.FactorioVersion.CompareTo(first.FactorioVersion);
            Updates = new CollectionView<ModUpdateViewModel>(updateVMs, CompareVersions);
        }

        private void Update()
        {
            DialogResult = DialogResult.Ok;
            AttachedView!.Close();
        }

        private void Cancel()
        {
            DialogResult = DialogResult.Cancel;
            AttachedView!.Close();
        }
    }
}
