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
    // This class houses setting properties we want to bind to independently from view models.
    internal sealed class LayoutSettings : NotifyPropertyChangedBase
    {
        private static readonly GridLength DefaultGridLength = new GridLength(1, GridUnitType.Star);
        private readonly SettingManager _manager;

        public GridLength ManagerGridLength1
        {
            get => _manager.Get(SettingName.ManagerGridLength1, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.ManagerGridLength1, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ManagerGridLength1)));
            }
        }

        public GridLength ManagerGridLength2
        {
            get => _manager.Get(SettingName.ManagerGridLength2, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.ManagerGridLength2, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ManagerGridLength2)));
            }
        }

        public GridLength OnlineGridLength1
        {
            get => _manager.Get(SettingName.OnlineGridLength1, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.OnlineGridLength1, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OnlineGridLength1)));
            }
        }

        public GridLength OnlineGridLength2
        {
            get => _manager.Get(SettingName.OnlineGridLength2, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.OnlineGridLength2, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OnlineGridLength2)));
            }
        }

        public GridLength OnlineSubGridLength1
        {
            get => _manager.Get(SettingName.OnlineSubGridLength1, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.OnlineSubGridLength1, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OnlineSubGridLength1)));
            }
        }

        public GridLength OnlineSubGridLength2
        {
            get => _manager.Get(SettingName.OnlineSubGridLength2, DefaultGridLength);
            set
            {
                _manager.Set(SettingName.OnlineSubGridLength2, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OnlineSubGridLength2)));
            }
        }

        public LayoutSettings(SettingManager manager)
            => _manager = manager;
    }
}
