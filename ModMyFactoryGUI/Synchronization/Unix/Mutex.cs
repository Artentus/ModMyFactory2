//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.IO;
using ModMyFactoryGUI;

namespace ModMyFactoryGUI.Synchronization.Unix
{
    internal sealed class Mutex : IUniversalMutex
    {
        private readonly FileInfo _lockFile;
        private bool _hasLock;
        private Stream _openStream;

        public Mutex()
        {
            var tempDir = Program.TemporaryDirectory;
            if (!tempDir.Exists) tempDir.Create();

            string path = Path.Combine(tempDir.FullName, "global_lock");
            _lockFile = new FileInfo(path);
            _hasLock = false;
        }

        public bool TryAquire()
        {
            if (_hasLock) return true;

            try
            {
                _hasLock = true;
                _openStream = _lockFile.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                _hasLock = false;
                _openStream?.Dispose();
                _openStream = null;
            }

            return _hasLock;
        }

        public void Release()
        {
            if (!_hasLock) throw new InvalidOperationException("Cannot release a mutex that has not been aquired");

            _hasLock = false;
            _openStream.Dispose();
            _openStream = null;
            _lockFile.Delete();
        }

        #region IDisposable Support

        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_hasLock) Release();
                _disposed = true;
            }
        }

        ~Mutex()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
