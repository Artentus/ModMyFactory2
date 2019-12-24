using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class MainWindowViewModel : ScreenBase<MainWindow>
    {
        public ICommand CloseCommand { get; }

        public ICommand OpenAboutWindowCommand { get; }

        public IEnumerable<CultureViewModel> AvailableCultures
            => App.Current.LocaleManager.AvailableCultures.Select(c => new CultureViewModel(c));

        public IEnumerable<ThemeViewModel> AvailableThemes
            => App.Current.ThemeManager.Themes.Select(t => new ThemeViewModel(t));

        public MainWindowViewModel(MainWindow window)
            : base(window)
        {
            CloseCommand = ReactiveCommand.Create(AttachedView.Close);
            OpenAboutWindowCommand = ReactiveCommand.CreateFromTask(OpenAboutWindow);
        }

        async Task OpenAboutWindow()
        {
            var window = View.CreateWithViewModel<AboutWindow, AboutWindowViewModel>(out var viewModel);
            await window.ShowDialog(AttachedView);
            viewModel.Dispose();
        }
    }
}
