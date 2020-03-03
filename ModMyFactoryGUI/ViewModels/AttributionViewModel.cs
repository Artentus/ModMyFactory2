//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using System.Collections.Generic;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class AttributionViewModel : RoutableViewModelBase<AttributionView>
    {
        public IReadOnlyList<WebLinkViewModel> Links { get; }

        public AttributionViewModel()
        {
            Links = new List<WebLinkViewModel>
            {
                new WebLinkViewModel("Avalonia UI", "https://avaloniaui.net/"),
                new WebLinkViewModel("ReactiveUI", "https://reactiveui.net/"),
                new WebLinkViewModel("Json.NET", "https://www.newtonsoft.com/json"),
                new WebLinkViewModel("Serilog", "https://serilog.net/"),
                new WebLinkViewModel("CommandLineParser", "https://github.com/commandlineparser/commandline"),
                new WebLinkViewModel("Markdig", "https://github.com/lunet-io/markdig"),
                new WebLinkViewModel("Rx.NET", "https://github.com/dotnet/reactive"),
                new WebLinkViewModel("Splat", "https://github.com/reactiveui/splat"),
                new WebLinkViewModel("SkiaSharp", "https://github.com/mono/SkiaSharp"),
                new WebLinkViewModel("JetBrains.Annotations", "https://www.nuget.org/packages/JetBrains.Annotations"),
                new WebLinkViewModel("Avalonia.ThemeManager", "https://github.com/wieslawsoltes/Avalonia.ThemeManager"),
                new WebLinkViewModel("SharpCompress", "https://github.com/adamhathcock/sharpcompress"),
                new WebLinkViewModel("Mono.Posix", "https://github.com/mono/mono/tree/master/mcs/class/Mono.Posix"),
                new WebLinkViewModel("SharpDX", "http://sharpdx.org/")
            };
        }
    }
}
