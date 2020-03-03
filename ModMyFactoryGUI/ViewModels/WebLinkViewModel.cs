//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ReactiveUI;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class WebLinkViewModel : ReactiveObject
    {
        public string LinkText { get; }

        public string Url { get; }

        public WebLinkViewModel(string linkText, string url)
            => (LinkText, Url) = (linkText, url);
    }
}
