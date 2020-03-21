//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi.Mods;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Tasks.Web
{
    internal abstract class DownloadJob : IJob
    {
        public FileInfo File { get; private set; }

        protected abstract Task<FileInfo> DownloadFile(CancellationToken cancellationToken, IProgress<double> progress);

        public async Task Run(CancellationToken cancellationToken, IProgress<double> progress)
        {
            File = await DownloadFile(cancellationToken, progress);
        }
    }

    internal sealed class DownloadModReleaseJob : DownloadJob
    {
        private readonly string _username, _token;

        public ModReleaseInfo Release { get; }

        public DownloadModReleaseJob(ModReleaseInfo release, string username, string token)
            => (Release, _username, _token) = (release, username, token);

        protected override Task<FileInfo> DownloadFile(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var dir = App.Current.Locations.GetModDir(Release.Info.FactorioVersion);
            string fileName = Path.Combine(dir.FullName, Release.FileName);
            return ModApi.DownloadModReleaseAsync(Release, _username, _token, fileName, cancellationToken, progress);
        }
    }
}
