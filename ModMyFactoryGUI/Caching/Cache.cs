//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Caching
{
    internal abstract class Cache<TKey, TValue> : IDisposable
        where TKey : IEquatable<TKey>
    {
        private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();
        private bool _disposed;

        protected abstract Task<TValue> ResolveCacheMiss(TKey key);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var val in _cache.Values)
                    {
                        if (val is IDisposable disp)
                            disp.Dispose();
                    }

                    _cache.Clear();
                }
                _disposed = true;
            }
        }

        public Task<TValue> QueryAsync(TKey key)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());

            if (_cache.TryGetValue(key, out TValue result))
                return Task.FromResult(result);

            return ResolveCacheMiss(key);
        }

        public void Clear()
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());

            _cache.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
