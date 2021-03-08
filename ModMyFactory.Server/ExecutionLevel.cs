//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;

namespace ModMyFactory.Server
{
    [JsonConverter(typeof(ExecutionLevelConverter))]
    public enum ExecutionLevel
    {
        /// <summary>
        /// Everyone can execute commands
        /// </summary>
        Everyone,

        /// <summary>
        /// Only admins can execute commands
        /// </summary>
        AdminsOnly,

        /// <summary>
        /// Noone can execute commands
        /// </summary>
        Noone
    }
}
