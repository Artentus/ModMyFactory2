using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class AboutWindowViewModel : ViewModelBase<AboutWindow>
    {
        public AboutWindowViewModel(AboutWindow window)
            : base(window)
        { }
    }
}
