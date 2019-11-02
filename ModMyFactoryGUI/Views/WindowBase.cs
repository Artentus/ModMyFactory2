using Avalonia.Controls;

namespace ModMyFactory.Gui.Views
{
    abstract class WindowBase : Window
    {
        protected WindowBase()
        {
            UseLayoutRounding = true;
        }
    }
}
