//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ModMyFactory;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Views
{
    internal class ManagerView : MainViewBase<ManagerViewModel>
    {
        private const string InternalFormat = "MMF_ICanEnableList";
        private const double MinDragDist = 8;

        private ListBoxItem? _modSourceItem, _modpackSourceItem;
        private Point? _dragStart;
        private volatile bool _dragging;

        public ManagerView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private ListBoxItem? GetPointerItem(ListBox listBox)
        {
            var children = listBox.GetVisualDescendants();
            foreach (var child in children)
            {
                if ((child is ListBoxItem item) && item.IsPointerOver)
                    return item;
            }

            return null;
        }

        private void PointerPressedHandler(object sender, PointerPressedEventArgs e)
        {
            if (sender is ListBox source)
            {
                _dragStart = e.GetPosition(source);
                if (source.Name == "ModList") _modSourceItem = GetPointerItem(source);
                if (source.Name == "ModpackList") _modpackSourceItem = GetPointerItem(source);
            }
        }

        private void PointerReleasedHandler(object sender, PointerReleasedEventArgs e)
        {
            _modSourceItem = null;
            _modpackSourceItem = null;
            _dragStart = null;
            _dragging = false;
        }

        private async Task ModListPointerMovedHandler(ListBox source, PointerEventArgs e)
        {
            if (_dragStart.HasValue)
            {
                var pos = e.GetPosition(source);
                var delta = pos - _dragStart.Value;
                var len = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
                if (len >= MinDragDist) // Only start dragging if the mouse has been moved a certain distance
                {
                    if (!(_modSourceItem is null)) // Only drag if mouse was over an item
                    {
                        _dragging = true;

                        var vms = source.SelectedItems.Cast<ModFamilyViewModel>().ToList();
                        var sourceVM = (ModFamilyViewModel)_modSourceItem.DataContext;
                        if (!vms.Contains(sourceVM)) vms.Add(sourceVM); // We need to check this in case the drag was initiated before the selection
                        _modSourceItem = null;

                        var list = new List<ICanEnable>(vms.Count);
                        foreach (var vm in vms)
                        {
                            // This should never fail
                            if (!(vm.SelectedModViewModel is null) && vm.Family.Contains(vm.SelectedModViewModel.Version, out var mod)) list.Add(mod);
                        }

                        var data = new DataObject();
                        data.Set(InternalFormat, list);
                        _ = await DragDrop.DoDragDrop(e, data, DragDropEffects.Link);

                        _dragStart = null;
                        _dragging = false;
                    }
                }
            }
        }

        private async Task ModpackListPointerMovedHandler(ListBox source, PointerEventArgs e)
        {
            if (_dragStart.HasValue)
            {
                var pos = e.GetPosition(source);
                var delta = pos - _dragStart.Value;
                var len = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
                if (len >= MinDragDist) // Only start dragging if the mouse has been moved a certain distance
                {
                    if (!(_modpackSourceItem is null)) // Only drag if mouse was over an item
                    {
                        var vms = source.SelectedItems.Cast<ModpackViewModel>().ToList();
                        if ((vms.Count > 0) && !(vms[0] is null))
                        {
                            var sourceVM = (ModpackViewModel)_modpackSourceItem.DataContext;
                            if (!sourceVM.IsRenaming) // Don't drag if the modpack is currently being renamed
                            {
                                _dragging = true;

                                if (!vms.Contains(sourceVM)) vms.Add(sourceVM); // We need to check this in case the drag was initiated before the selection
                                _modpackSourceItem = null;

                                var list = new List<ICanEnable>(vms.Count);
                                foreach (var vm in vms) list.Add(vm.Modpack);

                                var data = new DataObject();
                                data.Set(InternalFormat, list);
                                _ = await DragDrop.DoDragDrop(e, data, DragDropEffects.Link);

                                _dragStart = null;
                                _dragging = false;
                            }
                        }
                    }
                }
            }
        }

        private async void PointerMovedHandler(object sender, PointerEventArgs e)
        {
            if (!_dragging && (sender is ListBox source))
            {
                if (source.Name == "ModList") await ModListPointerMovedHandler(source, e);
                if (source.Name == "ModpackList") await ModpackListPointerMovedHandler(source, e);
            }
        }

        private void DragOverModListHandler(DragEventArgs e)
        {
            e.DragEffects = e.Data.Contains(DataFormats.FileNames)
                ? DragDropEffects.Link
                : DragDropEffects.None;
        }

        private void DragOverModpackListHandler(DragEventArgs e)
        {
            e.DragEffects = e.Data.Contains(InternalFormat)
                ? DragDropEffects.Link
                : DragDropEffects.None;
        }

        private void DragOverHandler(object sender, DragEventArgs e)
        {
            if (_dragging && (sender is ListBox target))
            {
                if (target.Name == "ModList") DragOverModListHandler(e);
                if (target.Name == "ModpackList") DragOverModpackListHandler(e);
            }
        }

        private async Task DropModListHandler(DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                var paths = e.Data.GetFileNames();
                foreach (var path in paths)
                {
                    if (FileHelper.PathExists(path))
                        await ViewModel!.ImportModAsync(path);
                }
            }
        }

        private  async Task DropModpackListHandler(ListBox target, DragEventArgs e)
        {
            if (e.Data.Contains(InternalFormat))
            {
                var item = GetPointerItem(target);
                var modpack = (item?.DataContext as ModpackViewModel)?.Modpack ?? Program.CreateModpack();

                var list = e.Data.Get<IList<ICanEnable>>(InternalFormat);
                modpack.AddRangeSafe(list);
                await Program.SaveModpacksAsync();
            }
        }

        private async void DropHandler(object sender, DragEventArgs e)
        {
            if (sender is ListBox target)
            {
                if (target.Name == "ModList") await DropModListHandler(e);
                if (target.Name == "ModpackList") await DropModpackListHandler(target, e);
            }

            _modSourceItem = null;
            _modpackSourceItem = null;
            _dragStart = null;
            _dragging = false;
        }

        public void ScrollModpackIntoView(ModpackViewModel vm)
        {
            var listBox = this.FindControl<ListBox>("ModpackList");
            listBox.ScrollIntoView(vm);
        }
    }
}
