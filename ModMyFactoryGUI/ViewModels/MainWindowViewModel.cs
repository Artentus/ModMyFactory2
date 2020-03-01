using Avalonia.Controls;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class MainWindowViewModel : ScreenBase<MainWindow>
    {
        readonly AboutWindowViewModel _aboutWindowViewModel = new AboutWindowViewModel();
        TabItem _selectedTab;
        IMainViewModel _selectedViewModel;

        public ICommand OpenAboutWindowCommand { get; }

        public IEnumerable<CultureViewModel> AvailableCultures
            => App.Current.LocaleManager.AvailableCultures.Select(c => new CultureViewModel(c));

        public IEnumerable<ThemeViewModel> AvailableThemes
            => App.Current.ThemeManager.Select(t => new ThemeViewModel(t));

        public ManagerViewModel ManagerViewModel { get; } = new ManagerViewModel();

        public OnlineModsViewModel OnlineModsViewModel { get; } = new OnlineModsViewModel();

        public FactorioViewModel FactorioViewModel { get; } = new FactorioViewModel();

        public SettingsViewModel SettingsViewModel { get; } = new SettingsViewModel();

        public IReadOnlyCollection<IControl> FileMenuItems { get; private set; }

        public bool FileMenuVisible => FileMenuItems.Count > 0;

        public IReadOnlyCollection<IControl> EditMenuItems { get; private set; }

        public bool EditMenuVisible => EditMenuItems.Count > 0;

        public TabItem SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (value != _selectedTab)
                {
                    _selectedTab = value;
                    this.RaisePropertyChanged(nameof(SelectedTab));
                    SelectedViewModel = GetViewModel(_selectedTab);
                }
            }
        }

        public IMainViewModel SelectedViewModel
        {
            get => _selectedViewModel;
            private set
            {
                if (value != _selectedViewModel)
                {
                    _selectedViewModel = value;
                    this.RaisePropertyChanged(nameof(SelectedViewModel));
                    RebuildMenuItems(_selectedViewModel);
                }
            }
        }

        public MainWindowViewModel()
        {
            OpenAboutWindowCommand = ReactiveCommand.CreateFromTask(OpenAboutWindow);
            SelectedViewModel = ManagerViewModel;
        }

        async Task OpenAboutWindow()
        {
            var window = View.CreateAndAttach(_aboutWindowViewModel);
            await window.ShowDialog(AttachedView);
        }

        IMainViewModel GetViewModel(TabItem tab)
        {
            if (tab.Content is IMainView view)
                return view.ViewModel;

            throw new ArgumentException("Tab does not contain a valid view", nameof(tab));
        }

        void RebuildMenuItems(IMainViewModel viewModel)
        {
            FileMenuItems = viewModel.FileMenuItems;
            this.RaisePropertyChanged(nameof(FileMenuItems));
            this.RaisePropertyChanged(nameof(FileMenuVisible));

            EditMenuItems = viewModel.EditMenuItems;
            this.RaisePropertyChanged(nameof(EditMenuItems));
            this.RaisePropertyChanged(nameof(EditMenuVisible));
        }
    }
}
