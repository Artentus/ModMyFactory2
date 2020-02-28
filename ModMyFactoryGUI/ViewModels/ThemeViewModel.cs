using Avalonia.Media.Imaging;
using Avalonia.ThemeManager;
using Avalonia.Utilities;
using ModMyFactoryGUI.Localization;
using ReactiveUI;
using System;
using System.IO;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class ThemeViewModel : ReactiveObject, IWeakSubscriber<EventArgs>
    {
        class EventManager
        {
            public event EventHandler SelectedThemeChanged;

            public void RaiseEvent()
                => SelectedThemeChanged?.Invoke(this, EventArgs.Empty);
        }


        const string ResourcePrefix = "__theme__.";
        static readonly EventManager InternalEventManager = new EventManager();

        public static void SubscribeWeak(IWeakSubscriber<EventArgs> subscriber)
            => WeakSubscriptionManager.Subscribe(InternalEventManager, nameof(EventManager.SelectedThemeChanged), subscriber);


        public ITheme Theme { get; }

        public string DisplayName => (string)App.Current.LocaleManager.GetResource(ResourcePrefix + Theme.Name);

        public IBitmap Icon { get; }

        public ICommand SelectCommand { get; }

        public bool Selected => App.Current.ThemeManager.SelectedTheme == Theme;

        public ThemeViewModel(ITheme theme)
        {
            Theme = theme;

            string iconPath = Path.Combine(App.Current.ApplicationDirectory.FullName,
                "themes", "assets", "icons", theme.Name + ".png");
            if (File.Exists(iconPath)) Icon = new Bitmap(iconPath);

            SelectCommand = ReactiveCommand.Create(Select);
            WeakSubscriptionManager.Subscribe(InternalEventManager, nameof(EventManager.SelectedThemeChanged), this);
            WeakSubscriptionManager.Subscribe(App.Current.LocaleManager, nameof(LocaleManager.UICultureChanged), this);
        }

        public void Select()
        {
            App.Current.ThemeManager.SelectedTheme = Theme;
            InternalEventManager.RaiseEvent();
        }

        void SelectedThemeChangedHandler() => this.RaisePropertyChanged(nameof(Selected));

        void UICultureChangedHandler() => this.RaisePropertyChanged(nameof(DisplayName));

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e)
        {
            if (sender is EventManager)
                SelectedThemeChangedHandler();
            else if (sender is LocaleManager)
                UICultureChangedHandler();
        }
    }
}
