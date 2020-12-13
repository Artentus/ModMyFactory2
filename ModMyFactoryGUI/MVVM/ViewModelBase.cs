//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ReactiveUI;
using System;

namespace ModMyFactoryGUI.MVVM
{
    internal abstract class ViewModelBase<T> : ReactiveObject where T : class, IView
    {
        public event EventHandler? ViewChanged;

        public T? AttachedView { get; private set; }

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
