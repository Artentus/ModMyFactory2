//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.ViewModels;
using ReactiveUI;
using System;

namespace ModMyFactoryGUI.Views
{
    internal interface IMainView : IView<IMainViewModel>, IControl
    { }

    internal abstract class MainViewBase<T> : ReactiveControlBase<T>, IMainView where T : class, IMainViewModel
    {
        IMainViewModel? IView<IMainViewModel>.ViewModel
        {
            get => ViewModel;
            set => ViewModel = value as T;
        }

        IReactiveObject? IView.ViewModel
        {
            get => ViewModel;
            set => ViewModel = value as T;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is IMainViewModel vm) vm.AttachView(this);
        }
    }
}
