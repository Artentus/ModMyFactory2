//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.ViewModels;

namespace ModMyFactoryGUI.Views
{
    interface IMainView : IView<IMainViewModel>, IControl
    { }

    abstract class MainViewBase<T> : ReactiveControlBase<T>, IMainView where T : class, IMainViewModel
    {
        IMainViewModel IView<IMainViewModel>.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (T)value;
        }
    }
}
