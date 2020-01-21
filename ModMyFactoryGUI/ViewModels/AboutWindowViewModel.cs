using Avalonia.Utilities;
using ModMyFactoryGUI.Localization;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    sealed class AboutWindowViewModel : ScreenBase<AboutWindow>, IWeakSubscriber<EventArgs>
    {
        public string Author => "Mathis Rech";

        public string GUIVersion => VersionStatistics.AppVersion;

        public IEnumerable<AssemblyVersionViewModel> AssemblyVersions { get; }

        public ICommand CloseCommand { get; }

        public AboutWindowViewModel()
        {
            WeakSubscriptionManager.Subscribe(App.Current.LocaleManager, nameof(LocaleManager.UICultureChanged), this);
            AssemblyVersions = VersionStatistics.LoadedAssemblyVersions.Select(kvp => new AssemblyVersionViewModel(kvp.Key, kvp.Value));
            CloseCommand = ReactiveCommand.Create(() => AttachedView.Close());
        }

        void UICultureChangedHandler(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(GUIVersion));
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e) => UICultureChangedHandler(sender, e);
    }
}
