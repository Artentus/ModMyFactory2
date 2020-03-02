using Avalonia.Controls;
using Avalonia.Controls.Platform;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactoryGUI.Controls
{
    abstract class ToolbarBase : MenuBase, IToolbar
    {
        IToolbarItem IToolbarElement.SelectedItem
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

        IEnumerable<IToolbarItem> IToolbarElement.SubItems
            => ItemContainerGenerator.Containers.Select(x => x.ContainerControl).OfType<IToolbarItem>();

        protected ToolbarBase()
        { }

        protected ToolbarBase(IMenuInteractionHandler interactionHandler)
            : base(interactionHandler)
        { }
    }
}
