//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System.ComponentModel;

namespace ModMyFactory.Server
{
    public struct ServerVisibility
    {
        /// <summary>
        /// If true, game will be published on the official Factorio matching server
        /// </summary>
        [JsonProperty("public")]
        [DefaultValue(true)]
        public bool IsPublic;

        /// <summary>
        /// if true, game will be broadcast on LAN
        /// </summary>
        [JsonProperty("lan")]
        [DefaultValue(true)]
        public bool ShowOnLan;
    }
}
