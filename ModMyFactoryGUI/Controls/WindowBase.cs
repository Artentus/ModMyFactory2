using Avalonia.Controls;
using ModMyFactoryGUI.MVVM;

namespace ModMyFactoryGUI.Controls
{
    abstract class WindowBase : Window, IView
    {
        protected WindowBase()
        {
            UseLayoutRounding = true;
        }
    }
}
