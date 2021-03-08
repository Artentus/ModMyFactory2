//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Tasks
{
    internal sealed class AsyncCollection<T>
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly List<T> _baseCollection = new List<T>();

        public async Task<int> CountAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                return _baseCollection.Count;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddAsync(T item)
        {
            await _semaphore.WaitAsync();

            try
            {
                _baseCollection.Add(item);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> RemoveAsync(T item)
        {
            await _semaphore.WaitAsync();

            try
            {
                return _baseCollection.Remove(item);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> ContainsAsync(T item)
        {
            await _semaphore.WaitAsync();

            try
            {
                return _baseCollection.Contains(item);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
