using Avalonia.Controls;
using ModMyFactoryGUI.MVVM;

namespace ModMyFactoryGUI.Controls
{
    abstract class WindowBase : Window, IView
    {
        public object ViewModel
        {
            get => DataContext;
            set => DataContext = value;
        }

        protected WindowBase()
        {
            UseLayoutRounding = true;
            App.Current.ThemeManager.EnableThemes(this);
        }
    }
}
