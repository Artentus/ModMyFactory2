using Avalonia.Controls;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactoryGUI.ViewModels
{
    abstract class MainViewModelBase<T> : RoutableViewModelBase<T>, IMainViewModel where T : class, IMainView
    {
        IReadOnlyCollection<IControl> _fileMenuItems, _editMenuItems;

        protected abstract List<IMenuItemViewModel> GetFileMenuViewModels();
        protected abstract List<IMenuItemViewModel> GetEditMenuViewModels();

        ICollection<IMenuItemViewModel> GetBaseFileMenuViewModels()
        {
            return new List<IMenuItemViewModel>
            {
                App.Current.ShutdownItemViewModel
            };
        }

        ICollection<IMenuItemViewModel> GetBaseEditMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }

        ICollection<IMenuItemViewModel> BuildMenuViewModelCollection(List<IMenuItemViewModel> items, ICollection<IMenuItemViewModel> baseItems)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));
            if (baseItems is null) throw new ArgumentNullException(nameof(baseItems));

            // Insert separator if both collections are non-empty
            if ((items.Count > 0) && (baseItems.Count > 0))
            {
                // Separator only appears in toolbar if both collections also appear in toolbar
                bool inToolbar = items.Any(i => i.IsInToolbar) && baseItems.Any(i => i.IsInToolbar);
                items.Add(new SeparatorMenuItemViewModel(inToolbar));
            }

            items.AddRange(baseItems);
            return items;
        }

        IControl CreateMenuItem(IMenuItemViewModel viewModel)
        {
            if (viewModel is SeparatorMenuItemViewModel)
            {
                return new Separator();
            }
            else if (viewModel is MenuItemViewModel itemViewModel)
            {
                return new MenuItem
                {
                    Header = itemViewModel.Header,
                    Icon = itemViewModel.Icon,
                    Command = itemViewModel.Command
                };
            }
            else
            {
                throw new ArgumentException("Unknown view model type", nameof(viewModel));
            }
        }

        IReadOnlyCollection<IControl> CreateMenuItems(ICollection<IMenuItemViewModel> viewModels)
        {
            var result = new List<IControl>(viewModels.Count);
            foreach (var viewModel in viewModels)
                result.Add(CreateMenuItem(viewModel));
            return result;
        }

        IReadOnlyCollection<IControl> BuildFileMenuItemCollection()
        {
            var items = GetFileMenuViewModels();
            var baseItems = GetBaseFileMenuViewModels();
            var viewModels = BuildMenuViewModelCollection(items, baseItems);
            return CreateMenuItems(viewModels);
        }

        IReadOnlyCollection<IControl> BuildEditMenuItemCollection()
        {
            var items = GetEditMenuViewModels();
            var baseItems = GetBaseEditMenuViewModels();
            var viewModels = BuildMenuViewModelCollection(items, baseItems);
            return CreateMenuItems(viewModels);
        }

        public IReadOnlyCollection<IControl> FileMenuItems
            => _fileMenuItems ??= BuildFileMenuItemCollection();

        public IReadOnlyCollection<IControl> EditMenuItems
            => _editMenuItems ??= BuildEditMenuItemCollection();
    }
}
