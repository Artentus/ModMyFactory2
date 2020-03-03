//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using ReactiveUI;
using System.Collections.Generic;

namespace ModMyFactoryGUI.ViewModels
{
    internal interface IMainViewModel : IRoutableViewModel
    {
        IReadOnlyCollection<IControl> FileMenuItems { get; }

        IReadOnlyCollection<IControl> EditMenuItems { get; }

        IReadOnlyCollection<IControl> ToolbarItems { get; }
    }
}
