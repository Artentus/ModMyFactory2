using WPFCore.Windows;

namespace ModMyFactory.Gui.Windows.Views
{
    abstract class WindowBase : ViewModelBoundWindow
    {
        protected WindowBase()
        {
            UseLayoutRounding = true;
        }
    }
}
