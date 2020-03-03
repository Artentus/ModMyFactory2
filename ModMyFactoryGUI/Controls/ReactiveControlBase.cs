//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.ReactiveUI;
using ModMyFactoryGUI.MVVM;
using ReactiveUI;

namespace ModMyFactoryGUI.Controls
{
    internal abstract class ReactiveControlBase<T> : ReactiveUserControl<T>, IView<T> where T : class, IRoutableViewModel
    {
        object IView.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (T)value;
        }
    }
}
