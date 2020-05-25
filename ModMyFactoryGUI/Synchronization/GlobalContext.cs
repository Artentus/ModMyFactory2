//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Synchronization
{
    internal class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageReceivedEventArgs(string message)
            => Message = message;
    }

    internal sealed class GlobalContext : IDisposable
    {
        private const string Uid = "ModMyFactoryGUI-uid(56F38E84-0DCD-475B-96F7-340ABBDEF8F1)";
        private readonly IUniversalMutex _globalMutex;
        private readonly SemaphoreSlim _syncSemaphore;
        private CancellationTokenSource _cancellationSource;
        private volatile bool _listening;
        private NamedPipeServerStream _currentPipe;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public static GlobalContext Current { get; } = new GlobalContext();

        public bool IsFirst { get; }

        private GlobalContext()
        {
            _globalMutex = UniversalMutex.CreateForCurrentPlatform(Uid);
            IsFirst = _globalMutex.TryAquire();

            _syncSemaphore = new SemaphoreSlim(1, 1);
        }

        private async Task ListenAsync()
        {
            try
            {
                var token = _cancellationSource.Token;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var pipe = new NamedPipeServerStream(Uid, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                        _currentPipe = pipe;
                        await pipe.WaitForConnectionAsync(token);

                        // Read message length
                        var lengthBuffer = new byte[sizeof(int)];
                        _ = await pipe.ReadAsync(lengthBuffer, 0, lengthBuffer.Length, token);
                        int length = BitConverter.ToInt32(lengthBuffer, 0);

                        // Read message as UTF8 string
                        var buffer = new byte[length];
                        _ = await pipe.ReadAsync(buffer, 0, length, token);
                        var message = Encoding.UTF8.GetString(buffer);

                        pipe.Disconnect();
                        pipe.Dispose();
                        _currentPipe = null;
                        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
                    }
                    catch (SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.OperationAborted) break;
                        else throw;
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        public void BeginListen()
        {
            if (IsFirst)
            {
                if (!_listening)
                {
                    _listening = true;
                    _cancellationSource = new CancellationTokenSource();

                    _syncSemaphore.Wait();
                    Task.Run(ListenAsync);
                }
            }
            else
            {
                throw new InvalidOperationException("Context does not allow for listening");
            }
        }

        public void EndListen()
        {
            if (IsFirst)
            {
                if (_listening)
                {
                    _cancellationSource.Cancel();
                    _currentPipe?.Dispose();

                    // Wait until the thread has safely exited
                    _syncSemaphore.Wait();
                    
                    _cancellationSource.Dispose();
                    _cancellationSource = null;
                    _listening = false;

                    _syncSemaphore.Release();
                }
            }
            else
            {
                throw new InvalidOperationException("Context does not allow for listening");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (IsFirst)
            {
                throw new InvalidOperationException("Context does not allow for sending");
            }
            else
            {
                using var pipe = new NamedPipeClientStream(".", Uid, PipeDirection.Out, PipeOptions.Asynchronous);
                try
                {
                    await pipe.ConnectAsync(10000); // Timeout in case something goes wrong

                    var buffer = Encoding.UTF8.GetBytes(message);
                    int length = buffer.Length;

                    // Write message length
                    var lengthBuffer = BitConverter.GetBytes(length);
                    await pipe.WriteAsync(lengthBuffer, 0, lengthBuffer.Length);

                    // Write message as UTF8 string
                    await pipe.WriteAsync(buffer, 0, length);
                    pipe.WaitForPipeDrain();
                }
                catch (TimeoutException)
                {
                    // Don't do anything since the server is not responding
                }
            }
        }

        #region IDisposable Support

        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (IsFirst) _globalMutex.Release();
                    _globalMutex.Dispose();
                    _syncSemaphore.Dispose();
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
