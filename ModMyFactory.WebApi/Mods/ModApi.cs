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
        internal const string BaseUrl = "https://mods.factorio.com";
        const string ModsUrl = BaseUrl + "/api/mods";

        /// <summary>
        /// Requests a mod page from the server.
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
        /// Requests extended information on a specific mod.
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
        /// Downloads a mod.
        /// </summary>
        /// <param name="release">The specific release to download.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="token">Login token for authentication.</param>
        /// <param name="file">Destination file.</param>
        public static async Task DownloadModReleaseAsync(ModReleaseInfo release, string username, string token, FileInfo file,
                                                         CancellationToken cancellationToken = default, IProgress<double> progress = null)
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
        /// Downloads a mod.
        /// </summary>
        /// <param name="release">The specific release to download.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="token">Login token for authentication.</param>
        /// <param name="fileName">Destination file name.</param>
        public static async Task<FileInfo> DownloadModReleaseAsync(ModReleaseInfo release, string username, string token, string fileName,
                                                         CancellationToken cancellationToken = default, IProgress<double> progress = null)
        {
            var file = new FileInfo(fileName);
            await DownloadModReleaseAsync(release, username, token, file, cancellationToken, progress);
            return file;
        }

        /// <summary>
        /// Generates a mods usable browser link.
        /// </summary>
        public static string GetModBrowserUrl(string modName) => $"{BaseUrl}/mod/{modName}";
    }
}
