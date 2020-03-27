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

    internal abstract class JobQueue<T>
        where T : class, IJob
    {
        private readonly AsyncQueue<T> _jobQueue = new AsyncQueue<T>();
        private readonly IProgress<(T, double)> _progress;
        private CancellationTokenSource _cancellationSource;
        private volatile bool _queueRunning = false;
        private T _currentJob;
        private volatile int _length;
        private volatile int _lengthInclusive;

        public event EventHandler<JobCompletedEventArgs<T>> JobCompleted;

        public event EventHandler LengthChanged;

        public int Length => _length;

        public bool IsJobInProgress => _lengthInclusive > 0;

        protected JobQueue(IProgress<(T, double)> progress)
            => _progress = progress;

        protected JobQueue()
            : this(new Progress<(T, double)>())
        { }

        private void OnJobProgress(object sender, double progress)
            => _progress.Report((_currentJob, progress));

        private async Task RunQueueAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _currentJob = await _jobQueue.Dequeue(cancellationToken).ConfigureAwait(false);

                // Job taken from queue, decrement queue length
                Interlocked.Decrement(ref _length);
                await OnLengthChanged(EventArgs.Empty);

                if (!cancellationToken.IsCancellationRequested)
                {
                    var jobProgress = _currentJob.Progress;
                    jobProgress.ProgressChanged += OnJobProgress;
                    await _currentJob.Run(cancellationToken).ConfigureAwait(false);
                    jobProgress.ProgressChanged -= OnJobProgress;

                    // Job completed, decrement inclusive queue length
                    Interlocked.Decrement(ref _lengthInclusive);
                    await OnJobCompleted(new JobCompletedEventArgs<T>(_currentJob));
                }
            }
        }

        protected virtual Task OnJobCompleted(JobCompletedEventArgs<T> e)
            => Dispatcher.UIThread.InvokeAsync(() => JobCompleted?.Invoke(this, e));

        protected virtual Task OnLengthChanged(EventArgs e)
            => Dispatcher.UIThread.InvokeAsync(() => LengthChanged?.Invoke(this, e));

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
            Interlocked.Increment(ref _length);
            Interlocked.Increment(ref _lengthInclusive);
            LengthChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task AddJobAsync(T job)
        {
            var source = new TaskCompletionSource<object>();

            void OnCompleted(object sender, JobCompletedEventArgs<T> e)
            {
                if (ReferenceEquals(e.Job, job))
                {
                    source.SetResult(null);
                    JobCompleted -= OnCompleted;
                }
            }

            JobCompleted += OnCompleted;

            await source.Task;
        }
    }
}
