//  Copyright (C) 2020 Mathis Rech
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
    internal sealed class ModUpdateWindowViewModel : ScreenBase<ModUpdateWindow>
    {
        public IReadOnlyDictionary<AccurateVersion, List<ModUpdateInfo>> Updates { get; }

        public ICommand UpdateCommand { get; }

        public ICommand CancelCommand { get; }

        public DialogResult DialogResult { get; private set; }

        public ModUpdateWindowViewModel(IReadOnlyDictionary<AccurateVersion, List<ModUpdateInfo>> updates)
        {
            Updates = updates;
            DialogResult = DialogResult.None;
            UpdateCommand = ReactiveCommand.Create(Update);
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        private void Update()
        {
            DialogResult = DialogResult.Ok;
            AttachedView.Close();
        }

        private void Cancel()
        {
            DialogResult = DialogResult.Cancel;
            AttachedView.Close();
        }
    }
}
