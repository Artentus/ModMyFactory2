//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ReactiveUI;
using System;

namespace ModMyFactoryGUI.MVVM
{
    internal interface IViewModel : IReactiveObject
    {
        IView? AttachedView { get; }
    }

    internal interface IViewModel<T> : IViewModel where T : class, IView
    {
        new T? AttachedView { get; }
    }

#pragma warning disable CS8612 // ReactiveObject doesn't implement PropertyCHanged with nullable=enable
    internal abstract class ViewModelBase<T> : ReactiveObject, IViewModel<T> where T : class, IView
#pragma warning restore CS8612 
    {
        public event EventHandler? ViewChanged;

        public T? AttachedView { get; private set; }

        IView? IViewModel.AttachedView => AttachedView;

        protected ViewModelBase()
        { }

        protected virtual void OnViewChanged(EventArgs e)
        {
            ViewChanged?.Invoke(this, e);
            this.RaisePropertyChanged(nameof(AttachedView));
        }

        public void AttachView(T view)
        {
            if (!(AttachedView is null))
                AttachedView.ViewModel = null;

            if (!(view is null))
                view.ViewModel = this;

            AttachedView = view;
            OnViewChanged(EventArgs.Empty);
        }
    }
}
