//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;

namespace ModMyFactory.ModSettings
{
    /// <summary>
    /// Defines when a mod setting is loaded.
    /// </summary>
    [JsonConverter(typeof(RuntimeTypeConverter))]
    public enum RuntimeType
    {
        /// <summary>
        /// The setting is loaded at game startup and can not be changed ingame.
        /// </summary>
        Startup,

        /// <summary>
        /// The setting affects all users in the game.
        /// </summary>
        Global,

        /// <summary>
        /// The setting only affects the local user.
        /// </summary>
        User
    }
}
