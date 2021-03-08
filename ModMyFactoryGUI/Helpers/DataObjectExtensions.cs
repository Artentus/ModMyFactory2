//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Input;

namespace ModMyFactoryGUI.Helpers
{
    internal static class DataObjectExtensions
    {
        public static T Get<T>(this IDataObject data, string dataFormat)
            => (T)data.Get(dataFormat);
    }
}
