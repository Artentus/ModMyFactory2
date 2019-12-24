using Avalonia.Media.Imaging;
using Avalonia.ThemeManager;
using ReactiveUI;
using System;
using System.IO;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class ThemeViewModel : ReactiveObject, IDisposable
    {
        static event EventHandler SelectedThemeChanged;

        public ITheme Theme { get; }

        public string DisplayName { get; }

        public IBitmap Icon { get; }

        public ICommand SelectCommand { get; }

        public bool Selected => App.Current.ThemeManager.SelectedTheme == Theme;

        public ThemeViewModel(ITheme theme)
        {
            Theme = theme;
            DisplayName = theme.Name;

            string iconPath = Path.Combine(App.Current.ApplicationDirectory.FullName,
                "themes", "assets", "icons", theme.Name + ".png");
            if (File.Exists(iconPath)) Icon = new Bitmap(iconPath);

            SelectCommand = ReactiveCommand.Create(Select);
            SelectedThemeChanged += SelectedThemeChangedHandler;
        }

        public void Select()
        {
            App.Current.ThemeManager.SelectedTheme = Theme;
            SelectedThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        void SelectedThemeChangedHandler(object sender, EventArgs e) => this.RaisePropertyChanged(nameof(Selected));


        private bool disposed = false;

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                if (disposing) Icon.Dispose();
                SelectedThemeChanged -= SelectedThemeChangedHandler;
            }
        }

        ~ThemeViewModel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
