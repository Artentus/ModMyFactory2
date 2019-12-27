using ReactiveUI;

namespace ModMyFactoryGUI.MVVM
{
    abstract class ScreenBase<T> : ViewModelBase<T>, IScreen where T : class, IView
    {
        public RoutingState Router { get; }

        protected ScreenBase()
        {
            Router = new RoutingState();
        }
    }
}
