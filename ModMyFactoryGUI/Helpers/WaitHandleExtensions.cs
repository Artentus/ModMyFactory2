//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal static class WaitHandleExtensions
    {
        public static Task WaitOneAsync(this WaitHandle waitHandle)
        {
            var source = new TaskCompletionSource<object?>();
            var registeredHandle = ThreadPool.RegisterWaitForSingleObject(waitHandle, (_, _) => source.SetResult(null), null, -1, true);
            var task = source.Task;
            task.ContinueWith((_) => registeredHandle.Unregister(null));
            return task;
        }
    }
}
