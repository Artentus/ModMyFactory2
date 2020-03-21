//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;

namespace ModMyFactory.WebApi.Mods
{
    /// <summary>
    /// Contains information about a mod
    /// </summary>
    public struct ApiModInfo
    {
        /// <summary>
        /// The display name of the mod
        /// </summary>
        [JsonProperty("title")]
        readonly public string DisplayName;

        /// <summary>
        /// How many times this mod has been downloaded
        /// </summary>
        [JsonProperty("downloads_count")]
        readonly public int DownloadCount;

        /// <summary>
        /// The unique name of this mod
        /// </summary>
        [JsonProperty("name")]
        readonly public string Name;

        /// <summary>
        /// A short description of the mod.
        /// Contains certain escaped characters like '\n'
        /// </summary>
        [JsonProperty("summary")]
        readonly public string Summary;

        /// <summary>
        /// The author of the mod.
        /// </summary>
        [JsonProperty("owner")]
        readonly public string Author;

        /// <summary>
        /// A weighted score value indicating the importance of this mod
        /// Specific implementation unknown
        /// </summary>
        [JsonProperty("score")]
        readonly public double Score;

        /// <summary>
        /// The category of the mod
        /// </summary>
        [JsonProperty("category")]
        readonly public string Category;

        /// <summary>
        /// The URL to this mods thumbnail
        /// </summary>
        [JsonProperty("thumbnail")]
        readonly public string ThumbnailUrl;

        /// <summary>
        /// The latest release of this mod
        /// </summary>
        [JsonProperty("latest_release")]
        readonly public ModReleaseInfo LatestRelease;


        // -------------- Extended data --------------

        /// <summary>
        /// A long description of the mod
        /// Only part of extended info
        /// Formatted in markdown
        /// </summary>
        [JsonProperty("description")]
        readonly public string Description;

        /// <summary>
        /// The mods changelog
        /// Only part of extended info
        /// Formatted in markdown
        /// </summary>
        [JsonProperty("changelog")]
        readonly public string Changelog;

        /// <summary>
        /// Frequently asked questions about the mod and their answers
        /// Only part of extended info
        /// Formatted in markdown
        /// </summary>
        [JsonProperty("faq")]
        readonly public string Faq;

        /// <summary>
        /// The mods homepage URL
        /// Only part of extended info
        /// </summary>
        [JsonProperty("homepage")]
        readonly public string Homepage;

        /// <summary>
        /// Url to the mods GitHub repository
        /// Only part of extended info
        /// </summary>
        [JsonProperty("github_path")]
        readonly public string GitHubUrl;

        /// <summary>
        /// The mods license
        /// Only part of extended info
        /// </summary>
        [JsonProperty("license")]
        readonly public License License;

        /// <summary>
        /// The releases of this mod
        /// Only part of extended info
        /// </summary>
        [JsonProperty("releases")]
        readonly public ModReleaseInfo[] Releases;

        /// <summary>
        /// The date at which the mod was first uploaded
        /// Only part of extended info
        /// </summary>
        [JsonProperty("created_at")]
        readonly public DateTime CreationDate;

        /// <summary>
        /// The last date this mod was updated at
        /// Only part of extended info
        /// </summary>
        [JsonProperty("updated_at")]
        readonly public DateTime LastUpdateDate;


        [JsonConstructor]
        internal ApiModInfo(string displayName, int downloadCount, string name, string summary, string author, double score, string category, string thumbnailUrl, ModReleaseInfo latestRelease,
                         string description, string changelog, string faq, string homepage, string gitHubUrl, License license, ModReleaseInfo[] releases, DateTime creationDate, DateTime lastUpdateDate)
        {
            DisplayName = displayName;
            DownloadCount = downloadCount;
            Name = name;
            Summary = summary;
            Author = author;
            Score = score;
            Category = category;

            if (!string.IsNullOrEmpty(thumbnailUrl) && thumbnailUrl.EndsWith("/.thumb.png")) ThumbnailUrl = null; // No thumbnail available
            else ThumbnailUrl = "https://mods-data.factorio.com" + thumbnailUrl;

            LatestRelease = latestRelease;


            // Extended data

            if (string.IsNullOrWhiteSpace(description)) Description = summary; // Use summary if no separate description available
            else Description = description;

            Changelog = changelog;
            Faq = faq;
            Homepage = homepage;
            GitHubUrl = gitHubUrl;
            License = license;
            Releases = releases;
            CreationDate = creationDate;
            LastUpdateDate = lastUpdateDate;
        }
    }
}
