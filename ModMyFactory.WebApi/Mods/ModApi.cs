//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactory.WebApi.Mods
{
    public static class ModApi
    {
        private const string ModsUrl = BaseUrl + "/api/mods";
        internal const string BaseUrl = "https://mods.factorio.com";

        /// <summary>
        /// Requests a mod page from the server
        /// </summary>
        /// <param name="pageSize">The page size. Negative values are interpreted as maximum page size (all mods on one page).</param>
        /// <param name="pageIndex">The 1-based index of the page to request based on the specified page size. Has no functionality if maximum page size is used.</param>
        public static async Task<ModPage> RequestPageAsync(int pageSize = -1, int pageIndex = 1)
        {
            if (pageSize == 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (pageIndex < 1) throw new ArgumentOutOfRangeException(nameof(pageIndex));

            string sizeStr = pageSize > 0 ? pageSize.ToString() : "max";
            string url = $"{ModsUrl}?page_size={sizeStr}";
            if (pageSize > 0) url += $"&page={pageIndex}";

            try
            {
                string document = await WebHelper.RequestDocumentAsync(url);
                return JsonConvert.DeserializeObject<ModPage>(document);
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
        }

        /// <summary>
        /// Requests extended information on a specific mod
        /// </summary>
        public static async Task<ApiModInfo> RequestModInfoAsync(string modName)
        {
            try
            {
                string url = $"{ModsUrl}/{modName}/full";
                string document = await WebHelper.RequestDocumentAsync(url);

                return JsonConvert.DeserializeObject<ApiModInfo>(document);
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
        }

        /// <summary>
        /// Downloads a mod
        /// </summary>
        /// <param name="release">The specific release to download</param>
        /// <param name="username">Username for authentication</param>
        /// <param name="token">Login token for authentication</param>
        /// <param name="file">Destination file</param>
        public static async Task DownloadModReleaseAsync(
            ModReleaseInfo release,
            string username, string token,
            FileInfo file,
            CancellationToken cancellationToken,
            IProgress<double> progress = null)
        {
            try
            {
                string url = $"{release.DownloadUrl}?username={username}&token={token}";
                await WebHelper.DownloadFileAsync(url, file, cancellationToken, progress);
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
        }

        /// <summary>
        /// Downloads a mod
        /// </summary>
        /// <param name="release">The specific release to download</param>
        /// <param name="username">Username for authentication</param>
        /// <param name="token">Login token for authentication</param>
        /// <param name="file">Destination file</param>
        public static Task DownloadModReleaseAsync(
            ModReleaseInfo release,
            string username, string token,
            FileInfo file)
            => DownloadModReleaseAsync(release, username, token, file, CancellationToken.None);

        /// <summary>
        /// Downloads a mod
        /// </summary>
        /// <param name="release">The specific release to download</param>
        /// <param name="username">Username for authentication</param>
        /// <param name="token">Login token for authentication</param>
        /// <param name="fileName">Destination file name</param>
        public static async Task<FileInfo> DownloadModReleaseAsync(
            ModReleaseInfo release,
            string username, string token,
            string fileName,
            CancellationToken cancellationToken,
            IProgress<double> progress = null)
        {
            var file = new FileInfo(fileName);
            await DownloadModReleaseAsync(release, username, token, file, cancellationToken, progress);
            return file;
        }

        /// <summary>
        /// Downloads a mod
        /// </summary>
        /// <param name="release">The specific release to download</param>
        /// <param name="username">Username for authentication</param>
        /// <param name="token">Login token for authentication</param>
        /// <param name="fileName">Destination file name</param>
        public static Task<FileInfo> DownloadModReleaseAsync(
            ModReleaseInfo release,
            string username, string token,
            string fileName)
            => DownloadModReleaseAsync(release, username, token, fileName, CancellationToken.None);

        /// <summary>
        /// Generates a usable browser link to the mods page
        /// </summary>
        public static string GetModBrowserUrl(string modName) => $"{BaseUrl}/mod/{modName}";
    }
}
