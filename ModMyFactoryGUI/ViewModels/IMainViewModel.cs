using Avalonia.Controls;
using ReactiveUI;
using System.Collections.Generic;

namespace ModMyFactoryGUI.ViewModels
{
    interface IMainViewModel : IRoutableViewModel
    {
        IReadOnlyCollection<IControl> FileMenuItems { get; }

        IReadOnlyCollection<IControl> EditMenuItems { get; }

        IReadOnlyCollection<IControl> ToolbarItems { get; }
    }
}
