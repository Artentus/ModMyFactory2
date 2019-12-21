using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class MainWindowViewModel : ViewModelBase<MainWindow>
    {
        public MainWindowViewModel(MainWindow window)
            : base(window)
        { }
    }
}
