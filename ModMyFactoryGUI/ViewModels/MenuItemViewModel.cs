using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    interface IMenuItemViewModel : IReactiveObject
    {
        bool IsInToolbar { get; }
    }

    abstract class MenuItemViewModelBase : ReactiveObject, IMenuItemViewModel
    {
        readonly Func<IControl> _iconFactory;

        public MenuHeaderViewModel Header { get; }

        public IControl Icon => _iconFactory.Invoke();

        public bool IsInToolbar { get; }

        protected MenuItemViewModelBase(bool isInToolbar, Func<IControl> iconFactory, string headerKey, string inputGestureKey = null)
        {
            (_iconFactory, IsInToolbar) = (iconFactory, isInToolbar);
            Header = new MenuHeaderViewModel(headerKey, inputGestureKey);
        }
    }

    class MenuItemViewModel : MenuItemViewModelBase
    {
        public ICommand Command { get; }

        public MenuItemViewModel(ICommand command, bool isInToolbar, Func<IControl> iconFactory, string headerKey, string inputGestureKey = null)
           : base(isInToolbar, iconFactory, headerKey, inputGestureKey)
        {
            Command = command;
        }
    }

    class ParentMenuItemViewModel : MenuItemViewModelBase
    {
        public IReadOnlyCollection<IMenuItemViewModel> SubItems { get; }

        public ParentMenuItemViewModel(IList<IMenuItemViewModel> subItems, bool isInToolbar, Func<IControl> iconFactory, string headerKey, string inputGestureKey = null)
            : base(isInToolbar, iconFactory, headerKey, inputGestureKey)
        {
            SubItems = new ReadOnlyCollection<IMenuItemViewModel>(subItems);
        }
    }

    class SeparatorMenuItemViewModel : ReactiveObject, IMenuItemViewModel
    {
        public bool IsInToolbar { get; }

        public SeparatorMenuItemViewModel(bool isInToolbar)
            => IsInToolbar = isInToolbar;
    }
}
