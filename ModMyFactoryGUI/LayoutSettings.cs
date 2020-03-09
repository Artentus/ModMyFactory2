//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using System.ComponentModel;

namespace ModMyFactoryGUI
{
    internal sealed class LayoutSettings : NotifyPropertyChangedBase
    {
        private static readonly GridLength DefaultGridLength = new GridLength(1, GridUnitType.Star);
        private readonly SettingManager _manager;

        public GridLength MainGridLength1
        {
            get => _manager.Get(SettingName.MainGridLength1, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.MainGridLength1, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(MainGridLength1)));
            }
        }

        public GridLength MainGridLength2
        {
            get => _manager.Get(SettingName.MainGridLength2, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.MainGridLength2, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(MainGridLength2)));
            }
        }

        public GridLength SubGridLength1
        {
            get => _manager.Get(SettingName.SubGridLength1, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.SubGridLength1, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SubGridLength1)));
            }
        }

        public GridLength SubGridLength2
        {
            get => _manager.Get(SettingName.SubGridLength2, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.SubGridLength2, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SubGridLength2)));
            }
        }

        public LayoutSettings(SettingManager manager)
            => _manager = manager;
    }
}
