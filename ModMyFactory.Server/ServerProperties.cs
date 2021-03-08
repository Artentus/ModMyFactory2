//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.Server
{
    /// <summary>
    /// Representation of a Factorio server properties file
    /// </summary>
    public sealed class ServerProperties
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Include | DefaultValueHandling.Populate
        };

        /// <summary>
        /// Name of the game as it will appear in the game listing
        /// </summary>
        [JsonProperty("name")]
        [DefaultValue("")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the game as it will appear in the listing
        /// </summary>
        [JsonProperty("description")]
        [DefaultValue("")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tags of the game as they will appear in the listing
        /// </summary>
        [JsonProperty("tags")]
        [DefaultValue(new string[0])]
        public string[] Tags { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Maximum number of players allowed<br/>
        /// A value of 0 means unlimited<br/>
        /// Admins can join even a full server 
        /// </summary>
        [JsonProperty("max_players")]
        [DefaultValue(0)]
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Visibility of the server to the outside
        /// </summary>
        [JsonProperty("visibility")]
        public ServerVisibility Visibility { get; set; }

        /// <summary>
        /// Valid factorio.com username<br/>
        /// Required for games with public visibility
        /// </summary>
        [JsonProperty("username")]
        [DefaultValue("")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Valid factorio.com password<br/>
        /// Required for games with public visibility
        /// </summary>
        [JsonProperty("password")]
        [DefaultValue("")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Authentication token<br/>
        /// May be used instead of a password for games with public visibility
        /// </summary>
        [JsonProperty("token")]
        [DefaultValue("")]
        public string AuthToken { get; set; } = string.Empty;

        /// <summary>
        /// Password required to join the game<br/>
        /// A value of null means the game will not be password protected
        /// </summary>
        [JsonProperty("game_password")]
        [DefaultValue("")]
        public string GamePassword { get; set; } = string.Empty;

        /// <summary>
        /// If true, the server will only allow clients that are logged in with a valid factorio.com account
        /// </summary>
        [JsonProperty("require_user_verification")]
        [DefaultValue(true)]
        public bool VerifyUsers { get; set; }

        /// <summary>
        /// Maximum upload bandwidth to be used in kilobytes per second<br/>
        /// A value of 0 means unlimited bandwidth
        /// </summary>
        [JsonProperty("max_upload_in_kilobytes_per_second")]
        [DefaultValue(0)]
        public int MaxUploadBandwidth { get; set; }

        /// <summary>
        /// Maximum number of concurrent uploads allowed<br/>
        /// A value of 0 means unlimited concurrent uploads
        /// </summary>
        [JsonProperty("max_upload_slots")]
        [DefaultValue(5)]
        public int MaxUploadSlots { get; set; }

        /// <summary>
        /// Minimum server latency in ticks<br/>
        /// One tick equals 16.6 milliseconds at normal game speed
        /// </summary>
        [JsonProperty("minimum_latency_in_ticks")]
        [DefaultValue(0)]
        public int MinTickLatency { get; set; }

        /// <summary>
        /// If true, players that played on the server already can join even when the player limit is reached
        /// </summary>
        [JsonProperty("ignore_player_limit_for_returning_players")]
        [DefaultValue(false)]
        public bool ReturningPlayersIgnoreLimit { get; set; }

        /// <summary>
        /// Defines who can execute server commands
        /// </summary>
        [JsonProperty("allow_commands")]
        [DefaultValue(ExecutionLevel.AdminsOnly)]
        public ExecutionLevel CommandPermissions { get; set; }

        /// <summary>
        /// Autosave interval in minutes
        /// </summary>
        [JsonProperty("autosave_interval")]
        [DefaultValue(10)]
        public int AutosaveInterval { get; set; }

        /// <summary>
        /// Maximum number of autosaves the server stores at a time<br/>
        /// If the maximum is reached, the oldest saves will be overwritten by new one
        /// </summary>
        [JsonProperty("autosave_slots")]
        [DefaultValue(5)]
        public int AutosaveCount { get; set; }

        /// <summary>
        /// Time in minutes until AFK players are kicked from the game
        /// </summary>
        [JsonProperty("afk_autokick_interval")]
        [DefaultValue(0)]
        public int AutokickDelay { get; set; }

        /// <summary>
        /// If true, the server will pause the game automatically if no players are online
        /// </summary>
        [JsonProperty("auto_pause")]
        [DefaultValue(true)]
        public bool AutoPause { get; set; }

        /// <summary>
        /// If true, only admins can pause the game
        /// </summary>
        [JsonProperty("only_admins_can_pause_the_game")]
        [DefaultValue(true)]
        public bool OnlyAdminsCanPause { get; set; }

        /// <summary>
        /// If true, autosaves are only stored on the server<br/>
        /// Otherwise all clients will also store the autosave
        /// </summary>
        [JsonProperty("autosave_only_on_server")]
        [DefaultValue(true)]
        public bool ServersideAutosaveOnly { get; set; }

        /// <summary>
        /// Experimental, use at own risk<br/>
        /// Only available on UNIX systems<br/>
        /// If true, the server will fork itself to create an autosave
        /// </summary>
        [JsonProperty("non_blocking_saving")]
        [DefaultValue(false)]
        public bool UseNonblockingAutosave { get; set; }

        [JsonProperty("minimum_segment_size")]
        [DefaultValue(25)]
        public int MinSegmentSize { get; set; }

        [JsonProperty("maximum_segment_size")]
        [DefaultValue(100)]
        public int MaxSegmentSize { get; set; }

        [JsonProperty("minimum_segment_size_peer_count")]
        [DefaultValue(20)]
        public int MinSegmentSizePeerCount { get; set; }

        [JsonProperty("maximum_segment_size_peer_count")]
        [DefaultValue(10)]
        public int MaxSegmentSizePeerCount { get; set; }

        /// <summary>
        /// Loads a server properties file
        /// </summary>
        public static ServerProperties Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<ServerProperties>(json);
        }

        /// <summary>
        /// Loads a server properties file
        /// </summary>
        public static ServerProperties Load(FileInfo file)
            => Load(file.FullName);

        /// <summary>
        /// Loads a server properties file
        /// </summary>
        public static async Task<ServerProperties> LoadAsync(FileInfo file)
        {
            using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(fs);
            var json = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<ServerProperties>(json);
        }

        /// <summary>
        /// Loads a server properties file
        /// </summary>
        public static Task<ServerProperties> LoadAsync(string filePath)
            => LoadAsync(new FileInfo(filePath));

        /// <summary>
        /// Saves a server properties file
        /// </summary>
        public void Save(string filePath)
        {
            var json = JsonConvert.SerializeObject(this, _jsonSettings);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// Saves a server properties file
        /// </summary>
        public void Save(FileInfo file)
            => Save(file.FullName);

        /// <summary>
        /// Saves a server properties file
        /// </summary>
        public async Task SaveAsync(FileInfo file)
        {
            var json = JsonConvert.SerializeObject(this, _jsonSettings);
            using var fs = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(fs);
            await writer.WriteAsync(json);
        }

        /// <summary>
        /// Saves a server properties file
        /// </summary>
        public Task SaveAsync(string filePath)
            => SaveAsync(new FileInfo(filePath));
    }
}
