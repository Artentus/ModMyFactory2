using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class MainWindowViewModel : ScreenBase<MainWindow>
    {
        public ICommand CloseCommand { get; }

        public ICommand OpenAboutWindowCommand { get; }

        public MainWindowViewModel(MainWindow window)
            : base(window)
        {
            CloseCommand = ReactiveCommand.Create(AttachedView.Close);
            
            var aboutWindow = View.CreateWithViewModel<AboutWindow, AboutWindowViewModel>(out var viewModel);
            OpenAboutWindowCommand = ReactiveCommand.CreateFromTask(() => aboutWindow.ShowDialog(AttachedView));
        }
    }
}
