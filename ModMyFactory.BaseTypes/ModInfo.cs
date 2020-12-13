//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// Information about a mod
    /// </summary>
    public struct ModInfo
    {
#pragma warning disable IDE0052

        // We store this only for the case that we want to save it again later on.
        [JsonProperty("factorio_version")]
        readonly private AccurateVersion ActualFactorioVersion;

#pragma warning restore IDE0052

        /// <summary>
        /// The unique name of the mod
        /// </summary>
        [JsonProperty("name")]
        readonly public string Name;

        /// <summary>
        /// The mods display name
        /// </summary>
        [JsonProperty("title")]
        readonly public string DisplayName;

        /// <summary>
        /// The mods version
        /// </summary>
        [JsonProperty("version")]
        readonly public AccurateVersion Version;

        /// <summary>
        /// The version of Factorio this mod works on<br/>
        /// This is always a major version, where version 1.0 is considered to be version 0.18 for compatibility reasons
        /// </summary>
        [JsonIgnore]
        readonly public AccurateVersion FactorioVersion;

        /// <summary>
        /// The author of the mod
        /// </summary>
        [JsonProperty("author")]
        readonly public string Author;

        /// <summary>
        /// A description of the mod
        /// </summary>
        [JsonProperty("description")]
        readonly public string Description;

        /// <summary>
        /// The dependencies of this mod
        /// </summary>
        [JsonProperty("dependencies")]
        [JsonConverter(typeof(SingleOrArrayConverter<Dependency>))]
        readonly public Dependency[] Dependencies;

        [JsonConstructor]
        internal ModInfo(string name, string displayName, AccurateVersion version, AccurateVersion actualFactorioVersion,
                         string author, string description, params Dependency[] dependencies)
            => (Name, DisplayName, Version, ActualFactorioVersion, FactorioVersion, Author, Description, Dependencies)
               = (name, displayName, version, actualFactorioVersion, actualFactorioVersion.ToFactorioMajor(), author, description, dependencies);

        /// <summary>
        /// Loads mod info from a json string
        /// </summary>
        public static ModInfo FromJson(string json)
            => JsonConvert.DeserializeObject<ModInfo>(json);

        /// <summary>
        /// Loads a mod info file
        /// </summary>
        public static async Task<ModInfo> FromFileAsync(FileInfo file)
        {
            using var fs = file.OpenRead();
            using var reader = new StreamReader(fs);
            string json = await reader.ReadToEndAsync();
            return FromJson(json);
        }

        /// <summary>
        /// Loads a mod info file
        /// </summary>
        public static Task<ModInfo> FromFileAsync(string fileName)
            => FromFileAsync(new FileInfo(fileName));

        /// <summary>
        /// Creates a json string from this mod info
        /// </summary>
        public string ToJson(Formatting formatting = Formatting.Indented, JsonSerializerSettings? settings = null)
            => JsonConvert.SerializeObject(this, formatting, settings);

        /// <summary>
        /// Saves a mod info file
        /// </summary>
        public async Task SaveToFileAsync(FileInfo file, Formatting formatting = Formatting.Indented, JsonSerializerSettings? settings = null)
        {
            if (!file.Directory.Exists) file.Directory.Create();
            using var fs = file.Open(FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(fs);
            string json = ToJson(formatting, settings);
            await writer.WriteAsync(json);
        }

        /// <summary>
        /// Saves a mod info file
        /// </summary>
        public Task SaveToFileAsync(string fileName, Formatting formatting = Formatting.Indented, JsonSerializerSettings? settings = null)
            => SaveToFileAsync(new FileInfo(fileName), formatting, settings);
    }
}
