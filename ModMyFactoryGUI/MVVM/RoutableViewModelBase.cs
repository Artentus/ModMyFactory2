//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace ModMyFactoryGUI.MVVM
{
    internal abstract class RoutableViewModelBase<T> : ViewModelBase<T>, IRoutableViewModel, IActivatableViewModel where T : class, IView
    {
        public IScreen HostScreen { get; }

        public string UrlPathSegment { get; }

        public ViewModelActivator Activator { get; }

        protected RoutableViewModelBase(IScreen hostScreen)
        {
            HostScreen = hostScreen;
            UrlPathSegment = Guid.NewGuid().ToString();

            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) =>
            {
                OnActivated();
                Disposable.Create(OnDeactivated).DisposeWith(disposables);
            });
        }

        protected RoutableViewModelBase()
            : this(null)
        { }

        protected virtual void OnActivated()
        { }

        protected virtual void OnDeactivated()
        { }
    }
}
