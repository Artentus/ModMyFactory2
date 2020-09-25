//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi;
using ModMyFactory.WebApi.Mods;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Caching.Web
{
    internal sealed class ModInfoCache : Cache<string, ApiModInfo?>
    {
        protected override async Task<ApiModInfo?> ResolveCacheMiss(string key)
        {
            try
            {
                return await ModApi.RequestModInfoAsync(key);
            }
            catch (ResourceNotFoundException)
            {
                return null;
            }
        }
    }
}
