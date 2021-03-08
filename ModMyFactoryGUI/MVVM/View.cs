//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ReactiveUI;

namespace ModMyFactoryGUI.MVVM
{
    internal interface IView
    {
        IReactiveObject? ViewModel { get; set; }
    }

    internal interface IView<T> : IView where T : IReactiveObject
    {
        new T? ViewModel { get; set; }
    }

    internal static class View
    {
        public static TView CreateAndAttach<TView>(ViewModelBase<TView> viewModel)
            where TView : class, IView, new()
        {
            var view = new TView();
            viewModel.AttachView(view);
            return view;
        }

        public static TViewModel? ViewModel<TViewModel>(this IView view)
            where TViewModel : ReactiveObject
        {
            return view.ViewModel as TViewModel;
        }
    }
}
