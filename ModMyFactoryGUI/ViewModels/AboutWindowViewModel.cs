using Avalonia.Utilities;
using ModMyFactoryGUI.Localization;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class AboutWindowViewModel : ScreenBase<AboutWindow>, IWeakSubscriber<EventArgs>
    {
        public string GUIVersion => App.GUIVersion;

        public string MMFVersion => App.MMFVersion;

        public AboutWindowViewModel()
        {
            WeakSubscriptionManager.Subscribe(App.Current.LocaleManager, nameof(LocaleManager.UICultureChanged), this);
        }

        void UICultureChangedHandler(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(GUIVersion));
            this.RaisePropertyChanged(nameof(MMFVersion));
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e) => UICultureChangedHandler(sender, e);
    }
}
