using Avalonia.Utilities;
using ModMyFactoryGUI.Localization;
using ReactiveUI;
using System;
using System.Reflection;

namespace ModMyFactoryGUI.ViewModels
{
    class AssemblyVersionViewModel : ReactiveObject, IWeakSubscriber<EventArgs>
    {
        public string AssemblyName { get; }

        public string AssemblyVersion { get; }

        public AssemblyVersionViewModel(Assembly assembly, string version)
        {
            AssemblyName = assembly.GetName().Name;
            AssemblyVersion = version;
            WeakSubscriptionManager.Subscribe(App.Current.LocaleManager, nameof(LocaleManager.UICultureChanged), this);
        }

        void UICultureChangedHandler(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(AssemblyVersion));
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e) => UICultureChangedHandler(sender, e);
    }
}
