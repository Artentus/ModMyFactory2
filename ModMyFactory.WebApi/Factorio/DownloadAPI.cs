using ModMyFactory.BaseTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactory.WebApi.Factorio
{
    public static class DownloadAPI
    {
        const string BaseUrl = "https://www.factorio.com";
        const string ReleasesUrl = BaseUrl + "/api/latest-releases";
        const string DownloadUrl = BaseUrl + "/get-download";

        /// <summary>
        /// Gets information about the latest stable and experimental releases of Factorio.
        /// </summary>
        public async static Task<(ReleaseInfo stable, ReleaseInfo experimental)> GetReleasesAsync()
        {
            try
            {
                string document = await WebHelper.RequestDocumentAsync(ReleasesUrl);

                dynamic response = JsonConvert.DeserializeObject(document);
                var stableDict = ((JToken)response.stable).ToObject<Dictionary<string, string>>();
                var experimentalDict = ((JToken)response.experimental).ToObject<Dictionary<string, string>>();
                return (new ReleaseInfo(stableDict), new ReleaseInfo(experimentalDict));
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
        }

        /// <summary>
        /// Downloads a specific release of Factorio.
        /// </summary>
        /// <param name="version">The version of Factorio.</param>
        /// <param name="build">The build of Factorio.</param>
        /// <param name="platform">The target platform.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="token">Login token for authentication.</param>
        /// <param name="file">The destination file.</param>
        public async static Task DownloadReleaseAsync(AccurateVersion version, FactorioBuild build, Platform platform, string username, string token, FileInfo file,
                                                      CancellationToken cancellationToken = default, IProgress<double> progress = null)
        {
            string versionStr = version.ToString(3);
            string buildStr = build.ToActualString();
            string platformStr = platform.ToActualString();
            string url = $"{DownloadUrl}/{versionStr}/{buildStr}/{platformStr}?username={username}&token={token}";

            try
            {
                await WebHelper.DownloadFileAsync(url, file, cancellationToken, progress);
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
        }

        /// <summary>
        /// Downloads a specific release of Factorio.
        /// </summary>
        /// <param name="version">The version of Factorio.</param>
        /// <param name="build">The build of Factorio.</param>
        /// <param name="platform">The target platform.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="token">Login token for authentication.</param>
        /// <param name="fileName">The destination file name.</param>
        public async static Task<FileInfo> DownloadReleaseAsync(AccurateVersion version, FactorioBuild build, Platform platform, string username, string token, string fileName,
                                                      CancellationToken cancellationToken = default, IProgress<double> progress = null)
        {
            var file = new FileInfo(fileName);
            await DownloadReleaseAsync(version, build, platform, username, token, file, cancellationToken, progress);
            return file;
        }
    }
}
