using Avalonia.Controls;

namespace ModMyFactoryGUI.Controls
{
    interface IToolbarItem : IToolbarElement, IMenuItem
    {
        new IToolbarElement Parent { get; }
    }
}
