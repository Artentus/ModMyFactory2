using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace ModMyFactoryGUI.MVVM
{
    abstract class RoutableViewModelBase<T> : ViewModelBase<T>, IRoutableViewModel, IActivatableViewModel where T : class, IView
    {
        public IScreen HostScreen { get; }

        public string UrlPathSegment { get; }

        public ViewModelActivator Activator { get; }

        protected RoutableViewModelBase(IScreen hostScreen)
        {
            HostScreen = hostScreen;
            UrlPathSegment = Guid.NewGuid().ToString();

            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) =>
            {
                OnActivated();
                Disposable.Create(OnDeactivated).DisposeWith(disposables);
            });
        }

        protected RoutableViewModelBase()
            : this(null)
        { }

        protected virtual void OnActivated()
        { }

        protected virtual void OnDeactivated()
        { }
    }
}
