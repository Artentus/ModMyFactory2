//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;

namespace ModMyFactory.WebApi.Mods
{
    /// <summary>
    /// Represents a software license
    /// </summary>
    public struct License
    {
        /// <summary>
        /// The name of the license
        /// </summary>
        [JsonProperty("name")]
        readonly public string Name;

        /// <summary>
        /// A URL to the license
        /// </summary>
        [JsonProperty("url")]
        readonly public string Url;

        [JsonConstructor]
        internal License(string name, string url)
            => (Name, Url) = (name, url);
    }
}
