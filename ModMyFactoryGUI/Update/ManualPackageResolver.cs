//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Onova.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Update
{
    sealed class ManualPackageResolver : IPackageResolver
    {
        private readonly string _updateFile;
        private readonly TagVersion _updateVersion;

        public ManualPackageResolver(string updateFile, TagVersion updateVersion)
            => (_updateFile, _updateVersion) = (updateFile, updateVersion);

        public Task DownloadPackageAsync(Version version, string destFilePath, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
            => Task.Run(() => File.Move(_updateFile, destFilePath));

        public Task<IReadOnlyList<Version>> GetPackageVersionsAsync(CancellationToken cancellationToken = default)
        {
            var version = new Version((int)_updateVersion.Major, (int)_updateVersion.Minor, (int)_updateVersion.Revision, (int)_updateVersion.Build);
            var result = new Version[] { version };
            return Task.FromResult<IReadOnlyList<Version>>(result);
        }
    }
}
