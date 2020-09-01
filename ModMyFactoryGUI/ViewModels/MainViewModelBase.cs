//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using System;

namespace ModMyFactoryGUI.ViewModels
{
    internal abstract class MainViewModelBase<T>
        : RoutableViewModelBase<T>, IMainViewModel
        where T : class, IMainView
    {
        void IMainViewModel.AttachView(IMainView view)
        {
            if (view is T v) AttachView(v);
            else throw new ArgumentException("Invalid view type", nameof(view));
        }
    }
}
