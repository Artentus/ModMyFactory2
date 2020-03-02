using Avalonia.Controls;
using Avalonia.Controls.Generators;

namespace ModMyFactoryGUI.Controls
{
    class ToolbarItemContainerGenerator : ItemContainerGenerator<ToolbarItem>
    {
        public ToolbarItemContainerGenerator(IControl owner)
            : base(owner, ToolbarItem.HeaderProperty, null)
        { }

        protected override IControl CreateContainer(object item)
            => item as Separator ?? base.CreateContainer(item);
    }
}
