//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.WebApi.Factorio;
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

        public abstract string Description { get; }

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

        public override string Description { get; }

        public DownloadModReleaseJob(ModReleaseInfo release, string modDisplayName, string username, string token)
            => (Release, Description, _username, _token) = (release, modDisplayName, username, token);

        protected override Task<FileInfo> DownloadFile(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var dir = Program.Locations.GetModDir(Release.Info.FactorioVersion);
            string fileName = Path.Combine(dir.FullName, Release.FileName);
            return ModApi.DownloadModReleaseAsync(Release, _username, _token, fileName, cancellationToken, progress);
        }
    }

    internal sealed class DownloadFactorioJob : DownloadJob
    {
        private readonly string _username, _token;
        private readonly AccurateVersion _version;
        private readonly FactorioBuild _build;
        private readonly Platform _platform;

        public override string Description => $"Factorio {_version}";

        public DownloadFactorioJob(string username, string token, AccurateVersion version, FactorioBuild build, Platform platform)
        {
            (_username, _token) = (username, token);
            (_version, _build, _platform) = (version, build, platform);
        }

        protected override Task<FileInfo> DownloadFile(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var dir = Program.TemporaryDirectory;
            string fileName = Path.Combine(dir.FullName, $"Factorio_{_version}.tmp"); // Dummy extension because it could be either ZIP or TAR.GZ
            return DownloadApi.DownloadReleaseAsync(_version, _build, _platform, _username, _token, fileName, cancellationToken, progress);
        }
    }
}
