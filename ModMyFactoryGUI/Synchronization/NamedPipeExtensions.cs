//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.IO.Pipes;
using System.Threading;

namespace ModMyFactoryGUI.Synchronization
{
    internal static class NamedPipeExtensions
    {
        // Workaround to allow the pipe server to shut down gracefully when cancelled during wating for connection
        public static bool WaitForConnectionEx(this NamedPipeServerStream server, string pipeName, CancellationToken cancellationToken)
        {
            var callbackEvent = new AutoResetEvent(false);
            Exception? innerEx = null;
            void callback(IAsyncResult result)
            {
                try
                {
                    server.EndWaitForConnection(result);
                }
                catch (Exception ex)
                {
                    innerEx = ex;
                }
                finally
                {
                    callbackEvent.Set();
                }
            }

            var result = server.BeginWaitForConnection(callback, null);

            bool cancelled = WaitHandle.WaitAny(new[] { result.AsyncWaitHandle, cancellationToken.WaitHandle }) == 1;
            if (cancelled)
            {
                // Dummy client to terminate the server wait
                using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                client.Connect();
                client.Close();
            }
            callbackEvent.WaitOne();

            if (!(innerEx is null)) throw innerEx;
            return !cancelled;
        }
    }
}
