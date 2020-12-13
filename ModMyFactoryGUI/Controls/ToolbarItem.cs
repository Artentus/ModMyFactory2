//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ModMyFactoryGUI.Controls
{
    internal interface IToolbarItem : IToolbarElement, IMenuItem
    {
        new IToolbarElement Parent { get; }
    }

    internal class ToolbarItem : HeaderedSelectingItemsControl, IToolbarItem, ISelectable
    {
        private class DependencyResolver : IAvaloniaDependencyResolver
        {
            /// <summary>
            /// Gets the default instance of <see cref="DependencyResolver"/>.
            /// </summary>
            public static readonly DependencyResolver Instance = new DependencyResolver();

            /// <summary>
            /// Gets a service of the specified type.
            /// </summary>
            /// <param name="serviceType">The service type.</param>
            /// <returns>A service of the requested type.</returns>
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IAccessKeyHandler))
                    return new MenuItemAccessKeyHandler();
                else
                    return AvaloniaLocator.Current.GetService(serviceType);
            }
        }


        private static readonly ITemplate<IPanel> DefaultPanel =
            new FuncTemplate<IPanel>(() => new StackPanel());

        private ICommand? _command;

        private bool _commandCanExecute = true;

        private Popup? _popup;

        public static readonly DirectProperty<ToolbarItem, ICommand?> CommandProperty =
            Button.CommandProperty.AddOwner<ToolbarItem>(
                toolbarItem => toolbarItem.Command,
                (toolbarItem, command) => toolbarItem.Command = command,
                enableDataValidation: true);

        public static readonly StyledProperty<object> CommandParameterProperty =
            Button.CommandParameterProperty.AddOwner<ToolbarItem>();

        public static readonly StyledProperty<object> IconProperty =
            MenuItem.IconProperty.AddOwner<ToolbarItem>();

        public static readonly StyledProperty<bool> IsSelectedProperty =
            ListBoxItem.IsSelectedProperty.AddOwner<ToolbarItem>();

        public static readonly StyledProperty<bool> IsSubMenuOpenProperty =
            MenuItem.IsSubMenuOpenProperty.AddOwner<ToolbarItem>();

        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
            RoutedEvent.Register<ToolbarItem, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<PointerEventArgs> PointerEnterItemEvent =
            RoutedEvent.Register<InputElement, PointerEventArgs>(nameof(PointerEnterItem), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<PointerEventArgs> PointerLeaveItemEvent =
            RoutedEvent.Register<InputElement, PointerEventArgs>(nameof(PointerLeaveItem), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> SubmenuOpenedEvent =
            RoutedEvent.Register<ToolbarItem, RoutedEventArgs>(nameof(SubmenuOpened), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs> Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public event EventHandler<PointerEventArgs> PointerEnterItem
        {
            add => AddHandler(PointerEnterItemEvent, value);
            remove => RemoveHandler(PointerEnterItemEvent, value);
        }

        public event EventHandler<PointerEventArgs> PointerLeaveItem
        {
            add => AddHandler(PointerLeaveItemEvent, value);
            remove => RemoveHandler(PointerLeaveItemEvent, value);
        }

        public event EventHandler<RoutedEventArgs> SubmenuOpened
        {
            add => AddHandler(SubmenuOpenedEvent, value);
            remove => RemoveHandler(SubmenuOpenedEvent, value);
        }

        public ICommand? Command
        {
            get => _command;
            set => SetAndRaise(CommandProperty, ref _command, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public bool IsSubMenuOpen
        {
            get => GetValue(IsSubMenuOpenProperty);
            set => SetValue(IsSubMenuOpenProperty, value);
        }

        public bool HasSubMenu => !Classes.Contains(":empty");

        public bool IsTopLevel => Parent is Toolbar;

        bool IMenuItem.IsPointerOverSubMenu => _popup?.IsPointerOverPopup ?? false;

        IMenuElement? IMenuItem.Parent => Parent as IMenuElement;

        IToolbarElement IToolbarItem.Parent => (IToolbarElement)Parent;

        protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;

        IMenuItem? IMenuElement.SelectedItem
        {
            get
            {
                var index = SelectedIndex;
                return (index != -1)
                    ? (IMenuItem)ItemContainerGenerator.ContainerFromIndex(index)
                    : null;
            }
            set => SelectedIndex = ItemContainerGenerator.IndexFromContainer(value);
        }

        IToolbarItem? IToolbarElement.SelectedItem
        {
            get
            {
                var index = SelectedIndex;
                return (index != -1)
                    ? (IToolbarItem)ItemContainerGenerator.ContainerFromIndex(index)
                    : null;
            }
            set => SelectedIndex = ItemContainerGenerator.IndexFromContainer(value);
        }

        IEnumerable<IMenuItem> IMenuElement.SubItems
            => ItemContainerGenerator.Containers.Select(x => x.ContainerControl).OfType<IMenuItem>();

        IEnumerable<IToolbarItem> IToolbarElement.SubItems
            => ItemContainerGenerator.Containers.Select(x => x.ContainerControl).OfType<IToolbarItem>();

        static ToolbarItem()
        {
            SelectableMixin.Attach<ToolbarItem>(IsSelectedProperty);
            CommandProperty.Changed.Subscribe(CommandChanged);
            FocusableProperty.OverrideDefaultValue<ToolbarItem>(true);
            HeaderProperty.Changed.AddClassHandler<ToolbarItem>((x, e) => x.HeaderChanged(e));
            IconProperty.Changed.AddClassHandler<ToolbarItem>((x, e) => x.IconChanged(e));
            IsSelectedProperty.Changed.AddClassHandler<ToolbarItem>((x, e) => x.IsSelectedChanged(e));
            ItemsPanelProperty.OverrideDefaultValue<ToolbarItem>(DefaultPanel);
            ClickEvent.AddClassHandler<ToolbarItem>((x, e) => x.OnClick(e));
            SubmenuOpenedEvent.AddClassHandler<ToolbarItem>((x, e) => x.OnSubmenuOpened(e));
            IsSubMenuOpenProperty.Changed.AddClassHandler<ToolbarItem>((x, e) => x.SubMenuOpenChanged(e));
        }

        private static void CommandChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is ToolbarItem toolbarItem)
            {
                if (((ILogical)toolbarItem).IsAttachedToLogicalTree)
                {
                    if (e.OldValue is ICommand oldCommand)
                        oldCommand.CanExecuteChanged -= toolbarItem.CanExecuteChanged;

                    if (e.NewValue is ICommand newCommand)
                        newCommand.CanExecuteChanged += toolbarItem.CanExecuteChanged;
                }

                toolbarItem.CanExecuteChanged(toolbarItem, EventArgs.Empty);
            }
        }

        private void CloseSubmenus()
        {
            foreach (var child in ((IToolbarItem)this).SubItems)
                child.IsSubMenuOpen = false;
        }

        private void CanExecuteChanged(object? sender, EventArgs e)
        {
            var canExecute = Command is null || Command.CanExecute(CommandParameter);

            if (canExecute != _commandCanExecute)
            {
                _commandCanExecute = canExecute;
                UpdateIsEffectivelyEnabled();
            }
        }

        private void HeaderChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is string newValue && newValue == "-")
            {
                PseudoClasses.Add(":separator");
                Focusable = false;
            }
            else if (e.OldValue is string oldValue && oldValue == "-")
            {
                PseudoClasses.Remove(":separator");
                Focusable = true;
            }
        }

        private void IconChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue is ILogical oldValue)
                LogicalChildren.Remove(oldValue);

            if (e.NewValue is ILogical newValue)
                LogicalChildren.Add(newValue);
        }

        private void IsSelectedChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                Focus();
        }

        private void SubMenuOpenChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;

            if (value)
            {
                RaiseEvent(new RoutedEventArgs(SubmenuOpenedEvent));
                IsSelected = true;
            }
            else
            {
                CloseSubmenus();
                SelectedIndex = -1;
            }
        }

        private void PopupOpened(object? sender, EventArgs e)
        {
            var selected = SelectedIndex;

            if (selected != -1)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(selected);
                container?.Focus();
            }
        }

        private void PopupClosed(object? sender, EventArgs e)
            => SelectedItem = null;

        protected override IItemContainerGenerator CreateItemContainerGenerator()
            => new ToolbarItemContainerGenerator(this);

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);

            if (!(Command is null))
                Command.CanExecuteChanged += CanExecuteChanged;
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);

            if (!(Command is null))
                Command.CanExecuteChanged -= CanExecuteChanged;
        }

        protected virtual void OnClick(RoutedEventArgs e)
        {
            if (!e.Handled && Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
                e.Handled = true;
            }
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            e.Handled = UpdateSelectionFromEventSource(e.Source, true);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Don't handle here: let event bubble up to toolbar.
        }

        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);

            var point = e.GetCurrentPoint(null);
            RaiseEvent(new PointerEventArgs(PointerEnterItemEvent, this, e.Pointer, this.VisualRoot, point.Position,
                e.Timestamp, point.Properties, e.KeyModifiers));
        }

        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerLeave(e);

            var point = e.GetCurrentPoint(null);
            RaiseEvent(new PointerEventArgs(PointerLeaveItemEvent, this, e.Pointer, this.VisualRoot, point.Position,
                e.Timestamp, point.Properties, e.KeyModifiers));
        }

        protected virtual void OnSubmenuOpened(RoutedEventArgs e)
        {
            if (e.Source is MenuItem menuItem && menuItem.Parent == this)
            {
                foreach (var child in ((IMenuItem)this).SubItems)
                {
                    if (child != menuItem && child.IsSubMenuOpen)
                        child.IsSubMenuOpen = false;
                }
            }
        }

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            if (!(_popup is null))
            {
                _popup.Opened -= PopupOpened;
                _popup.Closed -= PopupClosed;
                _popup.DependencyResolver = null;
            }

            _popup = e.NameScope.Find<Popup>("PART_Popup");

            if (!(_popup is null))
            {
                _popup.DependencyResolver = DependencyResolver.Instance;
                _popup.Opened += PopupOpened;
                _popup.Closed += PopupClosed;
            }
        }

        protected override void UpdateDataValidation(AvaloniaProperty property, BindingNotification status)
        {
            base.UpdateDataValidation(property, status);

            if (property == CommandProperty)
            {
                if (status.ErrorType == BindingErrorType.Error)
                {
                    if (_commandCanExecute)
                    {
                        _commandCanExecute = false;
                        UpdateIsEffectivelyEnabled();
                    }
                }
            }
        }

        public void Open() => IsSubMenuOpen = true;

        public void Close() => IsSubMenuOpen = false;

        bool IMenuElement.MoveSelection(NavigationDirection direction, bool wrap)
            => MoveSelection(direction, wrap);

        void IMenuItem.RaiseClick() => RaiseEvent(new RoutedEventArgs(ClickEvent));
    }
}
