using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class CultureViewModel : ReactiveObject, IDisposable
    {
        public CultureInfo Culture { get; }

        public string DisplayName { get; }

        public IBitmap Icon { get; }

        public ICommand SelectCommand { get; }

        public bool Selected => string.Equals(App.Current.LocaleManager.UICulture.TwoLetterISOLanguageName,
            Culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase);

        public CultureViewModel(CultureInfo culture)
        {
            Culture = culture;
            DisplayName = $"{culture.NativeName} ({culture.EnglishName})";

            string iconPath = Path.Combine(App.Current.ApplicationDirectory.FullName,
                "lang", "assets", "icons", culture.TwoLetterISOLanguageName + ".png");
            if (File.Exists(iconPath)) Icon = new Bitmap(iconPath);

            SelectCommand = ReactiveCommand.Create(Select);
            App.Current.LocaleManager.UICultureChanged += UICultureChangedHandler;
        }

        public void Select() => App.Current.LocaleManager.UICulture = Culture;

        void UICultureChangedHandler(object sender, EventArgs e) => this.RaisePropertyChanged(nameof(Selected));


        private bool disposed = false;

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                if (disposing) Icon.Dispose();
                App.Current.LocaleManager.UICultureChanged -= UICultureChangedHandler;
            }
        }

        ~CultureViewModel()
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
