//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal interface IMenuItemViewModel : IReactiveObject
    {
        bool IsInToolbar { get; }
    }

    internal interface IHeaderedItemViewModel : IMenuItemViewModel
    {
        MenuHeaderViewModel Header { get; }

        IControl Icon { get; }
    }

    internal interface ICommandItemViewModel : IHeaderedItemViewModel
    {
        ICommand Command { get; }

        KeyGesture Gesture { get; }
    }

    internal abstract class MenuItemViewModelBase : ReactiveObject, IHeaderedItemViewModel
    {
        private readonly Func<IControl> _iconFactory;

        public MenuHeaderViewModel Header { get; }

        public IControl Icon => _iconFactory.Invoke();

        public bool IsInToolbar { get; }

        protected MenuItemViewModelBase(bool isInToolbar, Func<IControl> iconFactory, string headerKey, string inputGestureKey = null)
        {
            (_iconFactory, IsInToolbar) = (iconFactory, isInToolbar);
            Header = new MenuHeaderViewModel(headerKey, inputGestureKey);
        }
    }

    internal class MenuItemViewModel : MenuItemViewModelBase, ICommandItemViewModel
    {
        public ICommand Command { get; }

        public KeyGesture Gesture { get; }

        public MenuItemViewModel(ICommand command, KeyGesture gesture, bool isInToolbar, Func<IControl> iconFactory, string headerKey, string inputGestureKey = null)
           : base(isInToolbar, iconFactory, headerKey, inputGestureKey)
        {
            Command = command;
            Gesture = gesture;
        }
    }

    internal class ParentMenuItemViewModel : MenuItemViewModelBase
    {
        public IReadOnlyCollection<IMenuItemViewModel> SubItems { get; }

        public ParentMenuItemViewModel(IList<IMenuItemViewModel> subItems, bool isInToolbar, Func<IControl> iconFactory, string headerKey, string inputGestureKey = null)
            : base(isInToolbar, iconFactory, headerKey, inputGestureKey)
        {
            SubItems = new ReadOnlyCollection<IMenuItemViewModel>(subItems);
        }
    }

    internal class SeparatorMenuItemViewModel : ReactiveObject, IMenuItemViewModel
    {
        public bool IsInToolbar { get; }

        public SeparatorMenuItemViewModel(bool isInToolbar)
            => IsInToolbar = isInToolbar;
    }
}
