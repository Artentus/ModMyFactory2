using Avalonia.Controls;
using ModMyFactoryGUI.MVVM;
using ReactiveUI;

namespace ModMyFactoryGUI.Controls
{
    abstract class WindowBase : Window, IView
    {
        object IView.ViewModel
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
