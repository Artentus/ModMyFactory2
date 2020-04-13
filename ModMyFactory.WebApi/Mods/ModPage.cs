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
    /// An API page containing a list of mods
    /// </summary>
    public struct ModPage
    {
        /// <summary>
        /// Information about the page
        /// </summary>
        /// <remarks>Null if this is the only page</remarks>
        [JsonProperty("pagination")]
        readonly public Pagination? Pagination;

        /// <summary>
        /// A list of mods
        /// </summary>
        [JsonProperty("results")]
        readonly public ApiModInfo[] Mods;

        [JsonConstructor]
        internal ModPage(Pagination? pagination, ApiModInfo[] mods)
            => (Pagination, Mods) = (pagination, mods);
    }
}
