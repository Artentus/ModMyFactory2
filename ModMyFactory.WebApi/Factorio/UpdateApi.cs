//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactory.WebApi.Factorio
{
    public static class UpdateApi
    {
        private const string BaseUrl = "https://updater.factorio.com";
        private const string PacketUrl = BaseUrl + "/get-available-versions";
        private const string DownloadUrl = BaseUrl + "/get-download-link";

        private async static Task<string?> GetPackageLinkAsync(Platform platform, AccurateVersion from, AccurateVersion to, string username, string token)
        {
            string url = $"{DownloadUrl}?apiVersion=2&username={username}&token={token}&package={platform.ToActualString()}&from={from}&to={to}";
            string document = await WebHelper.RequestDocumentAsync(url);
            var response = JsonConvert.DeserializeObject<string[]>(document);
            return response?[0];
        }

        /// <summary>
        /// Gets information on available update packages for a given platform.
        /// </summary>
        /// <param name="platform">The target platform.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="token">Login token for authentication.</param>
        public async static Task<IReadOnlyList<UpdatePackageInfo>> GetUpdatePackagesAsync(Platform platform, string username, string token)
        {
            try
            {
                string url = $"{PacketUrl}?apiVersion=2&username={username}&token={token}";
                string document = await WebHelper.RequestDocumentAsync(url);

                var response = JsonConvert.DeserializeObject<Dictionary<string, List<UpdatePackageInfo>>>(document);
                var packages = response?[platform.ToActualString()];
                if (packages is null) return Array.Empty<UpdatePackageInfo>();

                for (int i = packages.Count; i >= 0; i--)
                {
                    var package = packages[i];
                    if (package.From == package.To)
                        packages.RemoveAt(i);
                }

                return packages;
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
        }

        /// <summary>
        /// Downloads an update package.
        /// </summary>
        /// <param name="platform">The target platform.</param>
        /// <param name="from">The version to update from.</param>
        /// <param name="to">The version to update to.</param>
        /// <param name="file">The destination file.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="token">Login token for authentication.</param>
        public async static Task DownloadUpdatePackageAsync(
            Platform platform, AccurateVersion from, AccurateVersion to, FileInfo file, string username, string token,
            CancellationToken cancellationToken = default, IProgress<double>? progress = null)
        {
            try
            {
                string? url = await GetPackageLinkAsync(platform, from, to, username, token);
                if (url is null) throw new ResponseException();
                await WebHelper.DownloadFileAsync(url, file, cancellationToken, progress);
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
        }

        /// <summary>
        /// Downloads an update package.
        /// </summary>
        /// <param name="platform">The target platform.</param>
        /// <param name="packageInfo">Information of the version updating from and to.</param>
        /// <param name="file">The destination file.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="token">Login token for authentication.</param>
        public static Task DownloadUpdatePackageAsync(
            Platform platform, UpdatePackageInfo packageInfo, FileInfo file, string username, string token,
            CancellationToken cancellationToken = default, IProgress<double>? progress = null)
            => DownloadUpdatePackageAsync(platform, packageInfo.From, packageInfo.To, file, username, token, cancellationToken, progress);

        /// <summary>
        /// Downloads an update package.
        /// </summary>
        /// <param name="platform">The target platform.</param>
        /// <param name="from">The version to update from.</param>
        /// <param name="to">The version to update to.</param>
        /// <param name="fileName">The destination file name.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="token">Login token for authentication.</param>
        public async static Task<FileInfo> DownloadUpdatePackageAsync(
            Platform platform, AccurateVersion from, AccurateVersion to, string fileName, string username, string token,
            CancellationToken cancellationToken = default, IProgress<double>? progress = null)
        {
            var file = new FileInfo(fileName);
            await DownloadUpdatePackageAsync(platform, from, to, file, username, token, cancellationToken, progress);
            return file;
        }

        /// <summary>
        /// Downloads an update package.
        /// </summary>
        /// <param name="platform">The target platform.</param>
        /// <param name="packageInfo">Information of the version updating from and to.</param>
        /// <param name="fileName">The destination file name.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="token">Login token for authentication.</param>
        public static Task<FileInfo> DownloadUpdatePackageAsync(
            Platform platform, UpdatePackageInfo packageInfo, string fileName, string username, string token,
            CancellationToken cancellationToken = default, IProgress<double>? progress = null)
            => DownloadUpdatePackageAsync(platform, packageInfo.From, packageInfo.To, fileName, username, token, cancellationToken, progress);
    }
}
