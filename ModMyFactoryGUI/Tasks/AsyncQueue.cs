//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ModMyFactoryGUI.Tasks
{
    internal sealed class AsyncQueue<T>
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly BufferBlock<T> _buffer = new BufferBlock<T>();

        public void Enqueue(T item)
            => _buffer.Post(item);

        public async Task<T> Dequeue(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _buffer.ReceiveAsync(cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> Dequeue()
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _buffer.ReceiveAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
