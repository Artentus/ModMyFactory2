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
        public string GUIVersion => VersionStatistics.AppVersion;

        public AboutWindowViewModel()
        {
            WeakSubscriptionManager.Subscribe(App.Current.LocaleManager, nameof(LocaleManager.UICultureChanged), this);
        }

        void UICultureChangedHandler(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(GUIVersion));
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e) => UICultureChangedHandler(sender, e);
    }
}
