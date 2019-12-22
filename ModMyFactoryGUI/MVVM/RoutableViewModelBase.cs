using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace ModMyFactoryGUI.MVVM
{
    abstract class RoutableViewModelBase<T> : ViewModelBase<T>, IRoutableViewModel, IActivatableViewModel where T : IView
    {
        public IScreen HostScreen { get; }

        public string UrlPathSegment { get; }

        public ViewModelActivator Activator { get; }

        protected RoutableViewModelBase(T view, IScreen hostScreen)
            : base(view)
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

        protected RoutableViewModelBase(T view)
            : this(view, null)
        { }

        protected virtual void OnActivated()
        { }

        protected virtual void OnDeactivated()
        { }
    }
}
