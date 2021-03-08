//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.Controls;
using System;
using System.ComponentModel;

namespace ModMyFactoryGUI.Views
{
    partial class MainWindow : RestorableWindow
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (Program.Settings.Get(SettingName.MainWindowMaximized, false))
                WindowState = WindowState.Maximized;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Program.Settings.Set(SettingName.MainWindowMaximized, WindowState == WindowState.Maximized);
        }
    }
}
