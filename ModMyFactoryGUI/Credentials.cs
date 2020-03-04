//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;

namespace ModMyFactoryGUI
{
    internal readonly struct Credentials
    {
        [JsonProperty("username")]
        public string Username { get; }

        [JsonProperty("token")]
        public string Token { get; }

        [JsonConstructor]
        public Credentials(string username, string token)
            => (Username, Token) = (username, token);
    }
}
