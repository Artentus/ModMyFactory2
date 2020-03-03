//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ReactiveUI;

namespace ModMyFactoryGUI.MVVM
{
    abstract class ScreenBase<T> : ViewModelBase<T>, IScreen where T : class, IView
    {
        public RoutingState Router { get; }

        protected ScreenBase()
        {
            Router = new RoutingState();
        }
    }
}
