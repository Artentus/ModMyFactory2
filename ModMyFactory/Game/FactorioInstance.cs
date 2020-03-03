//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.Game
{
    public static class FactorioInstance
    {
        /// <summary>
        /// Converts this Factorio instance into a managed instance.
        /// If the instance is already managed it will be returned unaltered.
        /// </summary>
        public static ManagedFactorioInstance ToManaged(this IFactorioInstance instance) => ManagedFactorioInstance.FromInstance(instance);
    }
}
