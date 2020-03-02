using Avalonia.Controls;
using System.Collections.Generic;

namespace ModMyFactoryGUI.Controls
{
    interface IToolbarElement : IControl, IMenuElement
    {
        new IToolbarItem SelectedItem { get; set; }

        new IEnumerable<IToolbarItem> SubItems { get; }
    }
}
