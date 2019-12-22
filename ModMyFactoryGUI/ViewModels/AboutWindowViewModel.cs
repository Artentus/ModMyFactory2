using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class AboutWindowViewModel : ScreenBase<AboutWindow>
    {
        public AboutWindowViewModel(AboutWindow window)
            : base(window)
        { }
    }
}
