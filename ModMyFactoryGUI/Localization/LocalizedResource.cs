using System;
using System.ComponentModel;

namespace ModMyFactoryGUI.Localization
{
    sealed class LocalizedResource : NotifyPropertyChangedBase, IDisposable
    {
        string _key;

        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Value"));
            }
        }

        public object Value => App.Current.LocaleManager.GetResource(_key);

        public LocalizedResource(string key)
        {
            _key = key;
            App.Current.LocaleManager.UICultureChanged += UICultureChangedHandler;
        }

        void UICultureChangedHandler(object sender, EventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Value"));
        }


        bool disposed = false;

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                App.Current.LocaleManager.UICultureChanged -= UICultureChangedHandler;
            }
        }

        ~LocalizedResource()
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
