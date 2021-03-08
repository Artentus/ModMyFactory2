//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactoryGUI.Helpers
{
    internal static class NullableExtensions
    {
        public static bool IsTrue(this bool? nullable)
            => nullable.HasValue && nullable.Value;
    }
}
