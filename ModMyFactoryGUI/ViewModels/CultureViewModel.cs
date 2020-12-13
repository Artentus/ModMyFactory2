//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Media.Imaging;
using ModMyFactoryGUI.Helpers;
using ReactiveUI;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class CultureViewModel : ReactiveObject, IDisposable
    {
        private bool disposed = false;
        public CultureInfo Culture { get; }

        public string NativeName { get; }

        public string EnglishName { get; }

        public IBitmap? Icon { get; }

        public ICommand SelectCommand { get; }

        public bool Selected => string.Equals(App.Current.Locales.UICulture.Name, Culture.Name, StringComparison.OrdinalIgnoreCase);

        public CultureViewModel(CultureInfo culture)
        {
            Culture = culture;
            NativeName = culture.NativeName.Capitalize(culture);
            EnglishName = culture.EnglishName.Capitalize(culture);
            
            string iconPath = Path.Combine(Program.ApplicationDirectory.FullName,
                "lang", "assets", "icons", culture.Name + ".png");
            if (File.Exists(iconPath)) Icon = new Bitmap(iconPath);

            SelectCommand = ReactiveCommand.Create(Select);
            App.Current.Locales.UICultureChanged += UICultureChangedHandler;
        }

        ~CultureViewModel()
        {
            Dispose(false);
        }

        private void UICultureChangedHandler(object? sender, EventArgs e) => this.RaisePropertyChanged(nameof(Selected));

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                if (disposing) Icon?.Dispose();
                App.Current.Locales.UICultureChanged -= UICultureChangedHandler;
            }
        }

        public void Select() => App.Current.Locales.UICulture = Culture;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
