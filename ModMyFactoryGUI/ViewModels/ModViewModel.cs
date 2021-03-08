//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Media.Imaging;
using ModMyFactory.BaseTypes;
using ModMyFactory.Mods;
using ReactiveUI;
using System;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModViewModel : ReactiveObject, IDisposable
    {
        public Mod Mod { get; }

        public IBitmap? Thumbnail { get; }

        public AccurateVersion Version => Mod.Version;

        public ModViewModel(Mod mod)
        {
            Mod = mod;
            if (!(mod.Thumbnail is null))
                Thumbnail = new Bitmap(mod.Thumbnail);
        }

        #region IDisposable Support

        private bool disposed = false;

        ~ModViewModel()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    Thumbnail?.Dispose();

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
