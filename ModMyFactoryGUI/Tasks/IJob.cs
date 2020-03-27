//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Tasks
{
    internal interface IJob
    {
        Progress<double> Progress { get; }

        bool Success { get; }

        Task Run(CancellationToken cancellationToken);
    }

    internal static class Job
    {
        public static Task Run(this IJob job)
            => job.Run(CancellationToken.None);
    }
}
