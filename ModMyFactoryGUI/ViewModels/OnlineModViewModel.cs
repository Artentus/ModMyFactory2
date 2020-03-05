//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi.Mods;
using ReactiveUI;

namespace ModMyFactoryGUI.ViewModels
{
    internal class OnlineModViewModel : ReactiveObject
    {
        public ApiModInfo Info { get; }

        public string DisplayName => Info.DisplayName;

        public string Author => Info.Author;

        public int DownloadCount => Info.DownloadCount;

        public string Summary => Info.Summary;

        public OnlineModViewModel(ApiModInfo info)
            => Info = info;
    }
}
