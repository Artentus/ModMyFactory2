//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Tasks
{
    internal class JobCompletedEventArgs<T> : EventArgs
        where T : IJob
    {
        public T Job { get; }

        public JobCompletedEventArgs(T job)
            => Job = job;
    }

    internal abstract class JobManager<T>
        where T : IJob
    {
        private readonly AsyncQueue<T> _jobQueue = new AsyncQueue<T>();
        private readonly IProgress<(T, double)> _progress;
        private CancellationTokenSource _cancellationSource;
        private volatile bool _queueRunning = false;
        private T _currentJob;

        public event EventHandler<JobCompletedEventArgs<T>> JobCompleted;

        protected JobManager(IProgress<(T, double)> progress)
            => _progress = progress;

        protected JobManager()
            : this(new Progress<(T, double)>())
        { }

        private void OnJobProgress(double progress)
            => _progress.Report((_currentJob, progress));

        private async Task RunQueueAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _currentJob = await _jobQueue.Dequeue(cancellationToken).ConfigureAwait(false);

                if (!cancellationToken.IsCancellationRequested)
                {
                    var jobProgress = new Progress<double>(OnJobProgress);
                    await _currentJob.Run(cancellationToken, jobProgress).ConfigureAwait(false);
                    await OnJobCompleted(new JobCompletedEventArgs<T>(_currentJob));
                }
            }
        }

        protected virtual async Task OnJobCompleted(JobCompletedEventArgs<T> e)
            => await Dispatcher.UIThread.InvokeAsync(() => JobCompleted?.Invoke(this, e));

        public async void StartQueue()
        {
            if (!_queueRunning)
            {
                _queueRunning = true;
                _cancellationSource = new CancellationTokenSource();
                await RunQueueAsync(_cancellationSource.Token).ConfigureAwait(true);
            }

            throw new InvalidOperationException("Queue is already running");
        }

        public void StopQueue()
        {
            if (_queueRunning)
            {
                _cancellationSource?.Cancel();
                _queueRunning = false;
                return;
            }

            throw new InvalidOperationException("Queue is not running");
        }

        public void AddJob(in T job)
        {
            _jobQueue.Enqueue(job);
        }
    }
}
