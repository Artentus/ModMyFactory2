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
        readonly AboutWindowViewModel _aboutWindowViewModel = new AboutWindowViewModel();

        public ICommand CloseCommand { get; }

        public ICommand OpenAboutWindowCommand { get; }

        public IEnumerable<CultureViewModel> AvailableCultures
            => App.Current.LocaleManager.AvailableCultures.Select(c => new CultureViewModel(c));

        public IEnumerable<ThemeViewModel> AvailableThemes
            => App.Current.ThemeManager.Themes.Select(t => new ThemeViewModel(t));

        public MainWindowViewModel()
        {
            CloseCommand = ReactiveCommand.Create(CloseWindow);
            OpenAboutWindowCommand = ReactiveCommand.CreateFromTask(OpenAboutWindow);
        }

        void CloseWindow() => AttachedView.Close();

        async Task OpenAboutWindow()
        {
            var window = View.CreateAndAttach(_aboutWindowViewModel);
            await window.ShowDialog(AttachedView);
        }
    }
}
