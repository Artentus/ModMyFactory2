//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;

namespace ModMyFactoryGUI.ViewModels
{
    internal abstract class MainViewModelBase<T>
        : RoutableViewModelBase<T>, IMainViewModel
        where T : class, IMainView
    { }
}
