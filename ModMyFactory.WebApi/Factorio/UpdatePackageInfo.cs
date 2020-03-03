//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using Newtonsoft.Json;

namespace ModMyFactory.WebApi.Factorio
{
    /// <summary>
    /// Contains information about an update package.
    /// </summary>
    public struct UpdatePackageInfo
    {
        /// <summary>
        /// Version updating from.
        /// </summary>
        [JsonProperty("from")]
        public AccurateVersion From;

        /// <summary>
        /// Version updating to.
        /// </summary>
        [JsonProperty("to")]
        public AccurateVersion To;
    }
}
