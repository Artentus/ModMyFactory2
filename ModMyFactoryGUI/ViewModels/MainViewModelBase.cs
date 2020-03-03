//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Controls;
using ModMyFactoryGUI.Controls;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactoryGUI.ViewModels
{
    internal abstract class MainViewModelBase<T> : RoutableViewModelBase<T>, IMainViewModel where T : class, IMainView
    {
        private IReadOnlyCollection<IMenuItemViewModel> _fileMenuViewModels, _editMenuViewModels;
        private IReadOnlyCollection<IControl> _fileMenuItems, _editMenuItems, _toolbarItems;

        public IReadOnlyCollection<IControl> FileMenuItems
            => _fileMenuItems ??= BuildFileMenuItemCollection();

        public IReadOnlyCollection<IControl> EditMenuItems
            => _editMenuItems ??= BuildEditMenuItemCollection();

        public IReadOnlyCollection<IControl> ToolbarItems
            => _toolbarItems ??= BuildToolbarItemCollection();

        private static IReadOnlyCollection<IMenuItemViewModel> BuildMenuViewModelCollection(List<IMenuItemViewModel> items, ICollection<IMenuItemViewModel> baseItems)
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

        private static IControl CreateMenuItem(IMenuItemViewModel viewModel)
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
            else if (viewModel is ParentMenuItemViewModel parentViewModel)
            {
                return new MenuItem
                {
                    Header = parentViewModel.Header,
                    Icon = parentViewModel.Icon,
                    Items = CreateMenuItems(parentViewModel.SubItems)
                };
            }
            else
            {
                throw new ArgumentException("Unknown view model type", nameof(viewModel));
            }
        }

        private static IReadOnlyCollection<IControl> CreateMenuItems(IReadOnlyCollection<IMenuItemViewModel> viewModels)
        {
            var result = new List<IControl>(viewModels.Count);
            foreach (var viewModel in viewModels)
                result.Add(CreateMenuItem(viewModel));
            return result;
        }

        private static IControl CreateToolbarItem(IMenuItemViewModel viewModel, bool isTopLevel)
        {
            if (viewModel is SeparatorMenuItemViewModel)
            {
                return new Separator();
            }
            else if (viewModel is MenuItemViewModel itemViewModel)
            {
                var item = new ToolbarItem
                {
                    Header = itemViewModel.Header,
                    Icon = itemViewModel.Icon,
                    Command = itemViewModel.Command
                };

                if (isTopLevel)
                    item.SetValue(ToolTip.TipProperty, itemViewModel.Header);

                return item;
            }
            else if (viewModel is ParentMenuItemViewModel parentViewModel)
            {
                var item = new ToolbarItem
                {
                    Header = parentViewModel.Header,
                    Icon = parentViewModel.Icon,
                    Items = CreateToolbarItems(parentViewModel.SubItems)
                };

                if (isTopLevel)
                    item.SetValue(ToolTip.TipProperty, parentViewModel.Header);

                return item;
            }
            else
            {
                throw new ArgumentException("Unknown view model type", nameof(viewModel));
            }
        }

        private static IControl CreateToolbarItem(IMenuItemViewModel viewModel)
            => CreateToolbarItem(viewModel, false);

        private static IControl CreateTopLevelToolbarItem(IMenuItemViewModel viewModel)
            => CreateToolbarItem(viewModel, true);

        private static IReadOnlyCollection<IControl> CreateToolbarItems(
            IReadOnlyCollection<IMenuItemViewModel> viewModels)
        {
            var filteredViewModels = viewModels.Where(vm => vm.IsInToolbar);
            return filteredViewModels.Select(CreateToolbarItem).ToList();
        }

        private static IReadOnlyCollection<IControl> CreateToolbarItems(
            IReadOnlyCollection<IMenuItemViewModel> fileViewModels,
            IReadOnlyCollection<IMenuItemViewModel> editViewModels)
        {
            var filteredFileViewModels = fileViewModels.Where(vm => vm.IsInToolbar).ToList();
            var filteredEditViewModels = editViewModels.Where(vm => vm.IsInToolbar).ToList();

            bool addSeparator = (filteredFileViewModels.Count) > 0 && (filteredEditViewModels.Count > 0);
            int totalCount = filteredEditViewModels.Count + filteredEditViewModels.Count + (addSeparator ? 1 : 0);
            var result = new List<IControl>(totalCount);

            result.AddRange(filteredFileViewModels.ConvertAll(CreateTopLevelToolbarItem));
            if (addSeparator) result.Add(new Separator());
            result.AddRange(filteredEditViewModels.ConvertAll(CreateTopLevelToolbarItem));
            return result;
        }

        private ICollection<IMenuItemViewModel> GetBaseFileMenuViewModels()
        {
            return new List<IMenuItemViewModel>
            {
                App.Current.ShutdownItemViewModel
            };
        }

        private ICollection<IMenuItemViewModel> GetBaseEditMenuViewModels()
        {
            // ToDo: implement
            return new List<IMenuItemViewModel>();
        }

        private IReadOnlyCollection<IMenuItemViewModel> BuildFileMenuViewModelCollection()
        {
            var items = GetFileMenuViewModels();
            var baseItems = GetBaseFileMenuViewModels();
            return BuildMenuViewModelCollection(items, baseItems);
        }

        private IReadOnlyCollection<IMenuItemViewModel> BuildEditMenuViewModelCollection()
        {
            var items = GetEditMenuViewModels();
            var baseItems = GetBaseEditMenuViewModels();
            return BuildMenuViewModelCollection(items, baseItems);
        }

        private IReadOnlyCollection<IControl> BuildFileMenuItemCollection()
        {
            _fileMenuViewModels ??= BuildFileMenuViewModelCollection();
            return CreateMenuItems(_fileMenuViewModels);
        }

        private IReadOnlyCollection<IControl> BuildEditMenuItemCollection()
        {
            _editMenuViewModels ??= BuildEditMenuViewModelCollection();
            return CreateMenuItems(_editMenuViewModels);
        }

        private IReadOnlyCollection<IControl> BuildToolbarItemCollection()
        {
            _fileMenuViewModels ??= BuildFileMenuViewModelCollection();
            _editMenuViewModels ??= BuildEditMenuViewModelCollection();
            return CreateToolbarItems(_fileMenuViewModels, _editMenuViewModels);
        }

        protected abstract List<IMenuItemViewModel> GetFileMenuViewModels();

        protected abstract List<IMenuItemViewModel> GetEditMenuViewModels();
    }
}
