using ModMyFactory.BaseTypes;
using Newtonsoft.Json;
using System;

namespace ModMyFactory.WebApi.Mods
{
    /// <summary>
    /// Contains information on a mods release.
    /// </summary>
    public struct ModReleaseInfo
    {
        /// <summary>
        /// The version of this release.
        /// </summary>
        [JsonProperty("version")]
        readonly public AccurateVersion Version;

        /// <summary>
        /// The download URL of this release.
        /// </summary>
        [JsonProperty("download_url")]
        readonly public string DownloadUrl;

        /// <summary>
        /// The file name of the release.
        /// </summary>
        [JsonProperty("file_name")]
        readonly public string FileName;

        /// <summary>
        /// The date this release was uploaded at.
        /// </summary>
        [JsonProperty("released_at")]
        readonly public DateTime ReleaseDate;

        /// <summary>
        /// The SHA1 checksum of the file.
        /// </summary>
        [JsonProperty("sha1")]
        readonly public string Checksum;

        /// <summary>
        /// Select information from the mods info.
        /// </summary>
        [JsonProperty("info_json")]
        readonly public BaseTypes.ModInfo Info;

        [JsonConstructor]
        internal ModReleaseInfo(AccurateVersion version, string downloadUrl, string fileName, DateTime releaseDate, string checksum, BaseTypes.ModInfo info)
        {
            Version = version;
            DownloadUrl = ModApi.BaseUrl + downloadUrl;
            FileName = fileName;
            ReleaseDate = releaseDate;
            Checksum = checksum;
            Info = info;
        }
    }
}
