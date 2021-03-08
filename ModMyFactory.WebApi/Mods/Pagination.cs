//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;

namespace ModMyFactory.WebApi.Mods
{
    /// <summary>
    /// Contains information about the current page and all pages
    /// </summary>
    public struct Pagination
    {
        /// <summary>
        /// The total amount of pages at the current page size
        /// </summary>
        [JsonProperty("page_count")]
        readonly public int PageCount;

        /// <summary>
        /// The current page index
        /// </summary>
        [JsonProperty("page")]
        readonly public int PageIndex;

        /// <summary>
        /// The total amount of mods on all pages
        /// </summary>
        [JsonProperty("count")]
        readonly public int TotalModCount;

        /// <summary>
        /// The amount of mods on this page
        /// </summary>
        [JsonProperty("page_size")]
        readonly public int ModCount;

        [JsonConstructor]
        internal Pagination(int pageCount, int pageIndex, int totalModCount, int modCount)
            => (PageCount, PageIndex, TotalModCount, ModCount) = (pageCount, pageIndex, totalModCount, modCount);
    }
}
