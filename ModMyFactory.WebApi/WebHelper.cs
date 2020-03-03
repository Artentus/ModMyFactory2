//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactory.WebApi
{
    internal static class WebHelper
    {
        private const string UserAgent = "ModMyFactory_2";
        private const int BufferSize = 65536;

        private static async Task<HttpWebRequest> CreateHttpRequestAsync(Uri uri, byte[] content = null)
        {
            var request = WebRequest.CreateHttp(uri);
            request.KeepAlive = true;
            request.UserAgent = UserAgent;

            if ((content == null) || (content.Length == 0))
            {
                request.Method = WebRequestMethods.Http.Get;
            }
            else
            {
                request.Method = WebRequestMethods.Http.Post;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = content.Length;

                using var stream = await request.GetRequestStreamAsync();
                await stream.WriteAsync(content, 0, content.Length);
            }

            return request;
        }

        public async static Task<string> RequestDocumentAsync(Uri uri, byte[] content = null)
        {
            var request = await CreateHttpRequestAsync(uri, content);

            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = await request.GetResponseAsync();
                responseStream = response.GetResponseStream();

                using var reader = new StreamReader(responseStream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            finally
            {
                responseStream?.Close();
                response?.Close();
            }
        }

        public async static Task<string> RequestDocumentAsync(string url, byte[] content = null)
            => await RequestDocumentAsync(new Uri(url, UriKind.Absolute), content);

        public async static Task DownloadFileAsync(Uri uri, FileInfo file, CancellationToken cancellationToken = default, IProgress<double> progress = null)
        {
            var request = await CreateHttpRequestAsync(uri);

            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = await request.GetResponseAsync();
                responseStream = response.GetResponseStream();

                if (!file.Directory.Exists) file.Directory.Create();
                using var fs = file.Open(FileMode.Create, FileAccess.Write);

                long responseLength = response.ContentLength;
                long bytesWritten = 0;

                var buffer = new byte[BufferSize];
                int bytesRead = 0;
                do
                {
                    bytesRead = await responseStream.ReadAsync(buffer, 0, BufferSize, cancellationToken);
                    await fs.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    bytesWritten += bytesRead;

                    if ((responseLength > 0) && (progress != null))
                        progress.Report((double)bytesWritten / responseLength);
                } while (bytesRead == BufferSize);
            }
            catch (TaskCanceledException)
            {
                if (file.Exists) file.Delete();
            }
            catch
            {
                if (file.Exists) file.Delete();
                throw;
            }
            finally
            {
                responseStream?.Close();
                response?.Close();
            }
        }

        public async static Task DownloadFileAsync(string url, FileInfo file, CancellationToken cancellationToken = default, IProgress<double> progress = null)
            => await DownloadFileAsync(new Uri(url, UriKind.Absolute), file, cancellationToken, progress);
    }
}
