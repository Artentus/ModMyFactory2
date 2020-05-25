//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Diagnostics;
using System.Threading;

namespace ModMyFactoryGUI.Synchronization.Win32
{
    internal sealed class Mutex : IUniversalMutex
    {
        private readonly Semaphore _globalSemaphore;
        private bool _hasHandle;

        public Mutex(string uid)
        {
            // Make sure the semaphore name is absolutely unique for the current user
            string semaphoreName = $"Global\\{uid}-{Environment.MachineName}-sid{Process.GetCurrentProcess().SessionId}";
            _globalSemaphore = new Semaphore(1, 1, semaphoreName);
            _hasHandle = false;
        }

        public bool TryAquire()
        {
            if (_hasHandle) return true;

            try
            {
                _hasHandle = _globalSemaphore.WaitOne(100, false);
            }
            catch (AbandonedMutexException)
            {
                _hasHandle = true;
            }

            return _hasHandle;
        }

        public void Release()
        {
            _hasHandle = false;
            _globalSemaphore.Release();
        }

        #region IDisposable Support

        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _globalSemaphore.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}
