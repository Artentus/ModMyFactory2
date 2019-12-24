using ModMyFactory;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class AboutWindowViewModel : ScreenBase<AboutWindow>, IDisposable
    {
        public string GUIVersion => App.Version.ToString();

        public string MMFVersion => StaticInfo.Version.ToString();

        public AboutWindowViewModel(AboutWindow window)
            : base(window)
        {
            App.Current.LocaleManager.UICultureChanged += UICultureChangedHandler;
        }

        void UICultureChangedHandler(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(GUIVersion));
            this.RaisePropertyChanged(nameof(MMFVersion));
        }


        private bool disposed = false;

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                App.Current.LocaleManager.UICultureChanged -= UICultureChangedHandler;
            }
        }

        ~AboutWindowViewModel()
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
