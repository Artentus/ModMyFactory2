//  Copyright (C) 2020-2022 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System;

namespace ModMyFactoryGUI.Helpers
{
    internal static class WebHelper
    {
        private static readonly HttpClient _client = new();

        public static Task<string> DownloadStringAsync(Uri url)
            => _client.GetStringAsync(url);

        public static Task<string> DownloadStringAsync(string url)
            => _client.GetStringAsync(url);

        public static Task<byte[]> DownloadBytesAsync(Uri url)
            => _client.GetByteArrayAsync(url);

        public static Task<byte[]> DownloadBytesAsync(string url)
            => _client.GetByteArrayAsync(url);

        public static async Task DownloadFileAsync(Uri url, FileInfo file)
        {
            using var dst = file.Open(FileMode.Create);

            try
            {
                using var src = await _client.GetStreamAsync(url);
                await src.CopyToAsync(dst);
            }
            catch
            {
                if (file.Exists) file.Delete();
                throw;
            }
        }

        public static Task DownloadFileAsync(Uri url, string fileName)
        {
            var file = new FileInfo(fileName);
            return DownloadFileAsync(url, file);
        }

        public static async Task DownloadFileAsync(string url, FileInfo file)
        {
            using var dst = file.Open(FileMode.Create);
            
            try
            {
                using var src = await _client.GetStreamAsync(url);
                await src.CopyToAsync(dst);
            }
            catch
            {
                if (file.Exists) file.Delete();
                throw;
            }
        }

        public static Task DownloadFileAsync(string url, string fileName)
        {
            var file = new FileInfo(fileName);
            return DownloadFileAsync(url, file);
        }
    }
}
