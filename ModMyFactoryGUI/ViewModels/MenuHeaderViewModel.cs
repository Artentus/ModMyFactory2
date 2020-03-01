using System.ComponentModel;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class MenuHeaderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string HeaderKey { get; }

        public string InputGestureKey { get; }

        public bool InputGestureVisible { get; }

        public MenuHeaderViewModel(string headerKey, string inputGestureKey)
        {
            (HeaderKey, InputGestureKey) = (headerKey, inputGestureKey);
            InputGestureVisible = !string.IsNullOrEmpty(InputGestureKey);

            // This isn't a memory leak since the object stays alive for the entire application lifetime anyway
            App.Loaded += (s1, e1) =>
            {
                App.Current.LocaleManager.UICultureChanged += (s2, e2) =>
                {
                    // These properties don't really change, but the localization changes
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeaderKey)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(inputGestureKey)));
                };
            };
        }

        public MenuHeaderViewModel(string headerKey)
            : this(headerKey, null)
        { }
    }
}
