//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using Newtonsoft.Json;
using System;

namespace ModMyFactory.WebApi.Mods
{
    /// <summary>
    /// Contains information on a mods release
    /// </summary>
    public struct ModReleaseInfo
    {
        /// <summary>
        /// The version of this release
        /// </summary>
        [JsonProperty("version")]
        readonly public AccurateVersion Version;

        /// <summary>
        /// The download URL of this release
        /// </summary>
        [JsonProperty("download_url")]
        readonly public string DownloadUrl;

        /// <summary>
        /// The file name of the release
        /// </summary>
        [JsonProperty("file_name")]
        readonly public string FileName;

        /// <summary>
        /// The date this release was uploaded at
        /// </summary>
        [JsonProperty("released_at")]
        readonly public DateTime ReleaseDate;

        /// <summary>
        /// The SHA1 checksum of the file
        /// </summary>
        [JsonProperty("sha1")]
        [JsonConverter(typeof(SHA1HashConverter))]
        readonly public SHA1Hash Checksum;

        /// <summary>
        /// Select information from the mods info
        /// </summary>
        [JsonProperty("info_json")]
        readonly public ModInfo Info;

        [JsonConstructor]
        internal ModReleaseInfo(AccurateVersion version, string downloadUrl, string fileName,
            DateTime releaseDate, SHA1Hash checksum, ModInfo info)
            => (Version, DownloadUrl, FileName, ReleaseDate, Checksum, Info)
               = (version, ModApi.BaseUrl + downloadUrl, fileName, releaseDate, checksum, info);
    }
}
