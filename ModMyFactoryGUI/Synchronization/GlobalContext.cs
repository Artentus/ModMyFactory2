//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
        private CancellationTokenSource? _cancellationSource;
        private Task? _listenTask;
        private volatile bool _listening;

        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        public bool IsFirst { get; }

        private GlobalContext()
        {
            _globalMutex = UniversalMutex.CreateForCurrentPlatform(Uid);
            IsFirst = _globalMutex.TryAquire();
        }

        private void Listen(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (var pipe = new NamedPipeServerStream(Uid, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                    {
                        if (pipe.WaitForConnectionEx(Uid, token))
                        {
                            // Read message length
                            var lengthBuffer = new byte[sizeof(int)];
                            _ = pipe.Read(lengthBuffer, 0, lengthBuffer.Length);
                            int length = BitConverter.ToInt32(lengthBuffer, 0);

                            // Read message as UTF8 string
                            var buffer = new byte[length];
                            _ = pipe.Read(buffer, 0, length);
                            var message = Encoding.UTF8.GetString(buffer);

                            pipe.Disconnect();
                            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
                        }
                    }
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.OperationAborted) return;
                    else throw;
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        public static GlobalContext Create() => new GlobalContext();

        public void BeginListen()
        {
            if (IsFirst)
            {
                if (!_listening)
                {
                    _listening = true;
                    _cancellationSource = new CancellationTokenSource();
                    _listenTask = Task.Run(() => Listen(_cancellationSource.Token));
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
                    _cancellationSource!.Cancel();

                    // Wait until the thread has safely exited
                    _listenTask!.Wait();
                    _listenTask = null;

                    _cancellationSource.Dispose();
                    _cancellationSource = null;
                    _listening = false;
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
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
