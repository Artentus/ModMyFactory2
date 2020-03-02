using Avalonia.Controls;
using ReactiveUI;
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
        public MenuHeaderViewModel Header { get; }

        public IControl Icon { get; }

        public bool IsInToolbar { get; }

        protected MenuItemViewModelBase(bool isInToolbar, IControl icon, string headerKey, string inputGestureKey = null)
            => (Header, Icon, IsInToolbar) = (new MenuHeaderViewModel(headerKey, inputGestureKey), icon, isInToolbar);
    }

    class MenuItemViewModel : MenuItemViewModelBase
    {
        public ICommand Command { get; }

        public MenuItemViewModel(ICommand command, bool isInToolbar, IControl icon, string headerKey, string inputGestureKey = null)
           : base(isInToolbar, icon, headerKey, inputGestureKey)
        {
            Command = command;
        }
    }

    class ParentMenuItemViewModel : MenuItemViewModelBase
    {
        public IReadOnlyCollection<IMenuItemViewModel> SubItems { get; }

        public ParentMenuItemViewModel(IList<IMenuItemViewModel> subItems, bool isInToolbar, IControl icon, string headerKey, string inputGestureKey = null)
            : base(isInToolbar, icon, headerKey, inputGestureKey)
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
